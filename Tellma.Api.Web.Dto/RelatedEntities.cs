using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Tellma.Model.Admin;
using Tellma.Model.Application;
using Tellma.Model.Common;

namespace Tellma.Api.Dto
{
    public class RelatedEntities
    {
        public List<Account> Account { get; set; }
        public List<AccountClassification> AccountClassification { get; set; }
        public List<AccountType> AccountType { get; set; }
        public List<AdminUser> AdminUser { get; set; }
        public List<Agent> Agent { get; set; }
        public List<AgentDefinition> AgentDefinition { get; set; }
        public List<Center> Center { get; set; }
        public List<Currency> Currency { get; set; }
        public List<Document> Document { get; set; }
        public List<DocumentDefinition> DocumentDefinition { get; set; }
        public List<EntryForReconciliation> EntryForReconciliation { get; set; }
        public List<EntryType> EntryType { get; set; }
        public List<ExternalEntry> ExternalEntry { get; set; }
        public List<LineDefinition> LineDefinition { get; set; }
        public List<LineForQuery> LineForQuery { get; set; }
        public List<Lookup> Lookup { get; set; }
        public List<LookupDefinition> LookupDefinition { get; set; }
        public List<NotificationTemplate> NotificationTemplate { get; set; }
        public List<NotificationCommand> NotificationCommand { get; set; }
        public List<PrintingTemplate> PrintingTemplate { get; set; }
        public List<ReportDefinition> ReportDefinition { get; set; }
        public List<Resource> Resource { get; set; }
        public List<ResourceDefinition> ResourceDefinition { get; set; }
        public List<Role> Role { get; set; }
        public List<SqlServer> SqlServer { get; set; }
        public List<Unit> Unit { get; set; }
        public List<User> User { get; set; }
    }


    public static class RelatedEntitiesExtensions
    {
        private static Dictionary<string, Func<RelatedEntities, IList>> _getters;

        static RelatedEntitiesExtensions()
        {
            _getters = new Dictionary<string, Func<RelatedEntities, IList>>();

            foreach (var prop in typeof(RelatedEntities).GetProperties())
            {
                var getter = MakeGetter<RelatedEntities, IList>(prop.Name);
                _getters.Add(prop.Name, getter);
            }
        }

        private static Func<TEntity, TResult> MakeGetter<TEntity, TResult>(string propName)
        {
            var propInfo = typeof(TEntity).GetProperty(propName);

            var entityParam = Expression.Parameter(typeof(TEntity), "e"); // e
            var castEntity = Expression.Convert(entityParam, typeof(TEntity)); // (TEntity)e
            var memberAccess = Expression.MakeMemberAccess(castEntity, propInfo); // ((TEntity)e).User
            var castMemberAccess = Expression.Convert(memberAccess, typeof(TResult)); // (TResult)((TEntity)e).User
            var lambdaExp = Expression.Lambda<Func<TEntity, TResult>>(castMemberAccess, entityParam); // (e) => (TResult)((TEntity)e).User

            return lambdaExp.Compile();
        }

        private static void SetList(this RelatedEntities related, string name, IList list)
        {
            var propInfo = typeof(RelatedEntities).GetProperty(name);
            propInfo.SetValue(related, list);
        }

        private static IList GetList(this RelatedEntities related, string collection)
        {
            if (!_getters.TryGetValue(collection, out Func<RelatedEntities, IList> getter))
            {
                throw new ArgumentException($"The collection {collection} does not exist on {nameof(RelatedEntities)}.");
            }
            else
            {
                return getter(related);
            }
        }

        public static void AddEntity(this RelatedEntities relatedEntities, EntityWithKey entity)
        {
            if (entity == null)
            {
                return;
            }

            var type = entity.GetType();
            var desc = TypeDescriptor.Get(type);
            var collection = type.Name;

            var list = relatedEntities.GetList(collection);
            if (list == null)
            {
                list = desc.CreateList();
                relatedEntities.SetList(collection, list);
            }

            list.Add(entity);
        }

        public static IEnumerable<EntityWithKey> GetEntities(this RelatedEntities relatedEntities, string collection)
        {
            var list = relatedEntities.GetList(collection);
            if (list != null)
            {
                foreach (var item in list)
                {
                    yield return item as EntityWithKey;
                }
            }
        }
    }
}
