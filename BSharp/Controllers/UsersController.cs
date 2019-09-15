using BSharp.Controllers.Dto;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Data.Queries;
using BSharp.Entities;
using BSharp.Services.ApiAuthentication;
using BSharp.Services.Email;
using BSharp.Services.EmbeddedIdentityServer;
using BSharp.Services.MultiTenancy;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace BSharp.Controllers
{
    [Route("api/users")]
    [AuthorizeAccess]
    [ApplicationApi]
    public class UsersController : CrudControllerBase<UserForSave, User, int>
    {
        private readonly ApplicationRepository _appRepo;
        private readonly AdminRepository _adminRepo;
        private readonly ITenantIdAccessor _tenantIdProvider;
        private readonly Microsoft.AspNetCore.Mvc.ModelBinding.IModelMetadataProvider _metadataProvider;
        private readonly ILogger _logger;
        private readonly IEmailSender _emailSender;
        private readonly EmailTemplatesProvider _emailTemplates;
        private readonly GlobalOptions _options;
        private readonly IStringLocalizer _localizer;
        private readonly UserManager<EmbeddedIdentityServerUser> _userManager;

        // This is created and disposed across multiple methods
        private TransactionScope _adminTrxScope;
        private TransactionScope _identityTrxScope;

        public string VIEW => "users";

        public UsersController(
            ApplicationRepository appRepo,
            AdminRepository adminRepo,
            Microsoft.AspNetCore.Mvc.ModelBinding.IModelMetadataProvider metadataProvider,
            ILogger<UsersController> logger,
            IOptions<GlobalOptions> options,
            IServiceProvider serviceProvider,
            IEmailSender emailSender,
            EmailTemplatesProvider emailTemplates,
            IStringLocalizer<Strings> localizer,
            ITenantIdAccessor tenantIdProvider) : base(logger, localizer)
        {
            _appRepo = appRepo;
            _adminRepo = adminRepo;
            _tenantIdProvider = tenantIdProvider;
            _metadataProvider = metadataProvider;
            _logger = logger;
            _emailSender = emailSender;
            _emailTemplates = emailTemplates;
            _options = options.Value;
            _localizer = localizer;

            // we use this trick since this is an optional dependency, it will resolve to null if 
            // the embedded identity server is not enabled
            _userManager = (UserManager<EmbeddedIdentityServerUser>)serviceProvider.GetService(typeof(UserManager<EmbeddedIdentityServerUser>));
        }

        [HttpGet("client")]
        public async Task<ActionResult<DataWithVersion<UserSettingsForClient>>> UserSettingsForClient()
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                UserSettings userSettings = await _appRepo.Users__SettingsForClient();

                // prepare the result
                var forClient = new UserSettingsForClient
                {
                    UserId = userSettings.UserId,
                    Name = userSettings.Name,
                    Name2 = userSettings.Name2,
                    Name3 = userSettings.Name3,
                    ImageId = userSettings.ImageId,
                    CustomSettings = userSettings.CustomSettings
                };

                var result = new DataWithVersion<UserSettingsForClient>
                {
                    Version = userSettings.UserSettingsVersion.ToString(),
                    Data = forClient
                };

                return Ok(result);
            }, _logger);
        }

        [HttpPost("client")]
        public async Task<ActionResult<DataWithVersion<UserSettingsForClient>>> SaveUserSetting(
            [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))] [Required(ErrorMessage = nameof(RequiredAttribute))] string key,
            [StringLength(2048, ErrorMessage = nameof(StringLengthAttribute))] string value)
        {
            await _appRepo.Users__SaveSettings(key, value);

            return await UserSettingsForClient();
        }

        [HttpPut("invite")]
        public async Task<ActionResult> ResendInvitationEmail(int id)
        {
            return await ControllerUtilities.InvokeActionImpl((Func<Task<ActionResult>>)(async () =>
            {
                if (!_options.EmailEnabled)
                {
                    // Developer mistake
                    throw new BadRequestException("Email is not enabled in this installation");
                }

                if (!this._options.EmbeddedIdentityServerEnabled)
                {
                    // Developer mistake
                    throw new BadRequestException("Embedded identity is not enabled in this installation");
                }

                // Check if the user has permission
                await base.CheckActionPermissions("ResendInvitationEmail", id);

                // Load the user
                var user = await _appRepo.Users.Expand(nameof(Entities.User.Agent)).FilterByIds(id).FirstOrDefaultAsync();
                if (user == null)
                {
                    throw new NotFoundException<int>(id);
                }

                if (!string.IsNullOrWhiteSpace(user.ExternalId))
                {
                    throw new BadRequestException(_localizer["Error_User0HasAlreadyAcceptedTheInvitation", user.Email]);
                }

                string toEmail = user.Email;
                var idUser = await _userManager.FindByEmailAsync(toEmail);
                if (idUser == null)
                {
                    throw new NotFoundException<string>(toEmail);
                }

                if (idUser.EmailConfirmed)
                {
                    throw new BadRequestException(_localizer["Error_User0HasAlreadyAcceptedTheInvitation", user.Email]);
                }

                var (subject, htmlMessage) = await MakeInvitationEmailAsync(idUser, user.Agent);
                await _emailSender.SendEmailAsync(toEmail, subject, htmlMessage);
                return base.Ok();

            }), _logger);
        }

        private async Task<(string Subject, string Body)> MakeInvitationEmailAsync(EmbeddedIdentityServerUser identityRecipient, Agent recipient)
        {
            // Load the info
            var info = await _appRepo.GetTenantInfoAsync();

            // Use the recipient's preferred Language
            CultureInfo culture = new CultureInfo(recipient.PreferredLanguage);
            var localizer = _localizer.WithCulture(culture);

            // Prepare the parameters
            string userId = identityRecipient.Id;
            string emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(identityRecipient);
            string passwordToken = await _userManager.GeneratePasswordResetTokenAsync(identityRecipient);
            string nameOfInvitor =
                info.SecondaryLanguageId == culture.Name ? info.ShortCompanyName2 ?? info.ShortCompanyName :
                info.TernaryLanguageId == culture.Name ? info.ShortCompanyName3 ?? info.ShortCompanyName :
                info.ShortCompanyName;

            string nameOfRecipient =
                info.SecondaryLanguageId == culture.Name ? recipient.Name2 ?? recipient.Name :
                info.TernaryLanguageId == culture.Name ? recipient.Name3 ?? recipient.Name :
                recipient.Name;

            string callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId, code = emailToken, passwordCode = passwordToken, area = "Identity" },
                    protocol: Request.Scheme);

            // Prepare the email
            string emailSubject = localizer["InvitationEmailSubject0", localizer["AppName"]];
            string emailBody = _emailTemplates.MakeInvitationEmail(
                 nameOfRecipient: nameOfRecipient,
                 nameOfInvitor: nameOfInvitor,
                 validityInDays: Constants.TokenExpiryInDays,
                 userId: userId,
                 callbackUrl: callbackUrl,
                 culture: culture);

            return (emailSubject, emailBody);
        }

        protected override IRepository GetRepository()
        {
            return _appRepo;
        }

        protected override Query<User> Search(Query<User> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var email = nameof(Entities.User.Email);
                var name = nameof(Entities.User.Name);
                var name2 = nameof(Entities.User.Name2);
                var name3 = nameof(Entities.User.Name3);
                var cs = Ops.contains;

                query = query.Filter($"{name} {cs} '{search}' or {name2} {cs} '{search}' or {name3} {cs} '{search}' or {email} {cs} '{search}'");
            }

            return query;
        }

        protected override async Task SaveValidateAsync(List<UserForSave> entities)
        {
            // Hash the indices for performance
            var indices = entities.ToIndexDictionary();

            // Check that line ids are unique and that they have supplied a RoleId
            var duplicateLineIds = entities
                .SelectMany(e => e.Roles) // All lines
                .Where(e => e.Id != 0)
                .GroupBy(e => e.Id)
                .Where(g => g.Count() > 1) // Duplicate Ids
                .SelectMany(g => g)
                .ToDictionary(e => e, e => e.Id); // to dictionary

            foreach (var entity in entities)
            {
                var lineIndices = entity.Roles.ToIndexDictionary();
                foreach (var line in entity.Roles)
                {
                    if (duplicateLineIds.ContainsKey(line))
                    {
                        // This error indicates a bug
                        var index = indices[entity];
                        var lineIndex = lineIndices[line];
                        var id = duplicateLineIds[line];
                        ModelState.AddModelError($"[{index}].{nameof(entity.Roles)}[{lineIndex}].{nameof(entity.Id)}",
                            _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", id]);
                    }

                    if (line.RoleId == null)
                    {
                        var index = indices[entity];
                        var lineIndex = lineIndices[line];
                        var propName = nameof(RoleMembershipForSave.RoleId);
                        var propDisplayName = _metadataProvider.GetMetadataForProperty(typeof(RoleMembershipForSave), propName)?.DisplayName ?? propName;
                        ModelState.AddModelError($"[{index}].{nameof(entity.Roles)}[{lineIndex}].{nameof(RoleMembershipForSave.RoleId)}",
                            _localizer[nameof(RequiredAttribute), propDisplayName]);
                    }
                }
            }

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                return;
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _appRepo.Users_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override Task PreprocessSavedEntitiesAsync(List<UserForSave> entities)
        {
            // Make all the emails small case
            entities.ForEach(e => e.Email = e.Email.ToLower());
            return Task.CompletedTask;
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<UserForSave> entities, ExpandExpression expand, bool returnIds)
        {
            // NOTE: this method is not optimized for massive bulk (e.g. 1,000+ users), since it relies
            // on querying identity through UserManager one email at a time but it should be acceptable
            // with the usual workloads, customers with more than 200 users are rare anyways

            // Step (1) enlist the app repo
            _appRepo.EnlistTransaction(Transaction.Current); // So that it is not affected by admin trx scope later

            // Step (2): If Embedded Identity Server is enabled, create any emails that don't already exist there
            var usersToInvite = new List<(EmbeddedIdentityServerUser IdUser, UserForSave User)>();
            if (_options.EmbeddedIdentityServerEnabled)
            {
                _identityTrxScope = ControllerUtilities.CreateTransaction(TransactionScopeOption.RequiresNew);

                foreach (var entity in entities)
                {
                    var email = entity.Email;

                    // In case the user was added in a previous failed transaction
                    // or something, we always try to be forgiving in the code
                    var identityUser = await _userManager.FindByNameAsync(email) ??
                         await _userManager.FindByEmailAsync(email);

                    // This is truly a new user, create it
                    if (identityUser == null)
                    {
                        // Create the identity user
                        identityUser = new EmbeddedIdentityServerUser
                        {
                            UserName = email,
                            Email = email,
                            EmailConfirmed = !_options.EmailEnabled

                            // Note: If the system is integrated with an email service, user emails
                            // are automatically confirmed, otherwise users must confirm their 
                        };

                        var result = await _userManager.CreateAsync(identityUser);
                        if (!result.Succeeded)
                        {
                            string msg = string.Join(", ", result.Errors.Select(e => e.Description));
                            _logger.LogError(msg);

                            throw new BadRequestException($"An unexpected error occurred while creating an account for '{email}'");
                        }
                    }

                    // Mark for invitation later 
                    if (!identityUser.EmailConfirmed)
                    {
                        usersToInvite.Add((identityUser, entity));
                    }
                }
            }

            // Step (3): Save the users in the app database
            var (newEmails, oldEmails) = await _appRepo.Users__Save(entities);

            // Step (4) Same the emails in the admin database
            var tenantId = _tenantIdProvider.GetTenantId();
            _adminTrxScope = ControllerUtilities.CreateTransaction(TransactionScopeOption.RequiresNew);
            _adminRepo.EnlistTransaction(Transaction.Current);
            await _adminRepo.GlobalUsers__Save(newEmails, oldEmails, tenantId);

            // Step (5): Send the invitation emails
            if (usersToInvite.Any()) // This will be empty if email is disabled
            {
                var userIds = usersToInvite.Select(e => e.User.Id).ToArray();
                var agents = await _appRepo.Agents
                    .Select($"{nameof(Agent.Id)},{nameof(Agent.Name)},{nameof(Agent.Name2)},{nameof(Agent.Name3)},{nameof(Agent.PreferredLanguage)}")
                    .FilterByIds(userIds)
                    .ToListAsync();

                var agentsDic = agents.ToDictionary(e => e.Id);
                var tos = new List<string>();
                var subjects = new List<string>();
                var substitutions = new List<Dictionary<string, string>>();
                foreach (var (idUser, user) in usersToInvite)
                {
                    if (!agentsDic.TryGetValue(user.Id, out Agent agent))
                    {
                        // Programmer mistake
                        throw new InvalidOperationException($"User with id {user.Id} was saved but its corresponding Agent was not found");
                    }

                    // Add the email sender parameters
                    var (subject, body) = await MakeInvitationEmailAsync(idUser, agent);
                    tos.Add(idUser.Email);
                    subjects.Add(subject);
                    substitutions.Add(new Dictionary<string, string> { { "-message-", body } });
                }

                await _emailSender.SendEmailBulkAsync(
                    tos: tos,
                    subjects: subjects,
                    htmlMessage: $"-message-",
                    substitutions: substitutions.ToList()
                    );
            }

            // Return the same Ids that came
            return entities
                .Select(e => e.Id)
                .ToList();
        }

        protected override Task OnSaveCompleted()
        {
            if (_adminTrxScope != null)
            {
                _adminTrxScope.Complete();
                _adminTrxScope.Dispose();
            }

            if (_identityTrxScope != null)
            {
                _identityTrxScope.Complete();
                _identityTrxScope.Dispose();
            }

            return Task.CompletedTask;
        }

        protected override Task OnSaveError(Exception ex)
        {
            if (_adminTrxScope != null)
            {
                _adminTrxScope.Dispose();
            }

            if (_identityTrxScope != null)
            {
                _identityTrxScope.Dispose();
            }

            return Task.CompletedTask;
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // Make sure the user is not deleting his/her own account
            var userInfo = await _appRepo.GetUserInfoAsync();
            var index = ids.IndexOf(userInfo.UserId.Value);
            if (index >= 0)
            {
                ModelState.AddModelError($"[{index}]", _localizer["Error_CannotDeleteYourOwnUser"].Value);
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _appRepo.Users_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                // It's unfortunate that EF Core does not support distributed transactions, so there is no
                // guarantee that deletes to both the application and the admin will run one without the other
                using (var appTrx = ControllerUtilities.CreateTransaction())
                {
                    _appRepo.EnlistTransaction(Transaction.Current);
                    var oldEmails = await _appRepo.Users__Delete(ids);

                    using (var adminTrx = ControllerUtilities.CreateTransaction(TransactionScopeOption.RequiresNew))
                    {
                        var newEmails = new List<string>();
                        var tenantId = _tenantIdProvider.GetTenantId();

                        await _adminRepo.GlobalUsers__Save(newEmails, oldEmails, tenantId);

                        appTrx.Complete();
                        adminTrx.Complete();
                    }
                }
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["User"]]);
            }
        }

        protected override Query<User> GetAsQuery(List<UserForSave> entities)
        {
            return _appRepo.Users__AsQuery(entities);
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return _appRepo.UserPermissions(action, VIEW);
        }
    }
}
