﻿using GeoJSON.Net;
using GeoJSON.Net.Contrib.Wkb;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Tellma.Api.Dto;
using Tellma.Api.ImportExport;
using Tellma.Api.Templating;
using Tellma.Model.Application;
using Tellma.Model.Common;
using Tellma.Utilities.Common;

namespace Tellma.Api.Base
{
    public static class BaseUtil
    {
        /// <summary>
        /// Helper function that returns the <see cref="CultureInfo"/> that corresponds
        /// to the given <paramref name="name"/>, or the current UI culture if name was null.
        /// </summary>
        /// <param name="name">The culture name, for example "en".</param>
        /// <exception cref="ServiceException">If <paramref name="name"/> is not null and invalid.</exception>
        public static CultureInfo GetCulture(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return CultureInfo.CurrentUICulture;
            }

            try
            {
                return new CultureInfo(name);
            }
            catch (CultureNotFoundException)
            {
                throw new ServiceException($"Value '{name}' could not be interpreted as a valid culture.");
            }
        }

        /// <summary>
        /// Extract the collection <see cref="QueryInfo"/> used as context for 
        /// template generation based on a collection of entities.
        /// </summary>
        public static QueryInfo EntitiesPreloadedQuery<TKey>(PrintEntitiesArguments<TKey> args, string collection, int? defId)
        {
            // Preloaded Query
            QueryInfo preloadedQuery;
            if (args.I != null && args.I.Any())
            {
                preloadedQuery = new QueryEntitiesByIdsInfo(
                    collection: collection,
                    definitionId: defId,
                    ids: args.I);
            }
            else
            {
                preloadedQuery = new QueryEntitiesInfo(
                    collection: collection,
                    definitionId: defId,
                    filter: args.Filter,
                    orderby: args.OrderBy,
                    top: args.Top,
                    skip: args.Skip);
            }

            return preloadedQuery;
        }

        /// <summary>
        /// Create the local variables dictionary used for template generation 
        /// based on a collection of entities.
        /// </summary>
        public static Dictionary<string, EvaluationVariable> EntitiesLocalVariables<TKey>(PrintEntitiesArguments<TKey> args, string collection, int? defId)
        {
            return new Dictionary<string, EvaluationVariable>
            {
                ["$Source"] = new EvaluationVariable($"{collection}/{defId}"),
                ["$Filter"] = new EvaluationVariable(args.Filter),
                ["$OrderBy"] = new EvaluationVariable(args.OrderBy),
                ["$Top"] = new EvaluationVariable(args.Top),
                ["$Skip"] = new EvaluationVariable(args.Skip),
                ["$Ids"] = new EvaluationVariable(args.I)
            };
        }

        /// <summary>
        /// Creates an all-string local variables dictionary from the values in <see cref="PrintArguments.Custom"/> of <paramref name="args"/>.
        /// <para>
        /// The keys added are only those given in <paramref name="paramKeys"/>.
        /// </para>
        /// </summary>
        public static Dictionary<string, EvaluationVariable> CustomLocalVariables(PrintArguments args, IEnumerable<string> paramKeys)
        {
            var result = new Dictionary<string, EvaluationVariable>();

            if (args.Custom != null && paramKeys != null)
            {
                foreach (var key in paramKeys)
                {
                    args.Custom.TryGetValue(key, out string value);
                    result.Add(key, new EvaluationVariable(value));
                }
            }

            return result;
        }

        /// <summary>
        /// Extract the collection <see cref="QueryInfo"/> used as context for 
        /// template generation based on a single entity.
        /// </summary>
        public static QueryInfo EntityPreloadedQuery(object id, PrintEntityByIdArguments args, string collection, int? defId)
        {
            // Preloaded Query
            QueryInfo preloadedQuery = new QueryEntityByIdInfo(
                    collection: collection,
                    definitionId: defId,
                    id: id);

            return preloadedQuery;
        }

        /// <summary>
        /// Create the local variables dictionary used for template generation 
        /// based on a single entity.
        /// </summary>
        public static Dictionary<string, EvaluationVariable> EntityLocalVariables(object id, PrintEntityByIdArguments args, string collection, int? defId)
        {
            return new Dictionary<string, EvaluationVariable>
            {
                ["$Source"] = new EvaluationVariable(defId == null ? collection : $"{collection}/{defId}"),
                ["$Id"] = new EvaluationVariable(id),
            };
        }

        /// <summary>
        /// Takes an XLSX or a CSV stream and unpackages its content into a 2-D table of strings.
        /// </summary>
        /// <param name="stream">The contents of the XLSX or CSV file.</param>
        /// <param name="fileName">The name of the file to extract if available.</param>
        /// <param name="contentType">The mime type of the file to extract if available.</param>
        /// <param name="localizer">To localize error messages.</param>
        /// <returns>A 2-D grid of strings representing the contents of the XLSX or the CSV file.</returns>
        public static IEnumerable<string[]> ExtractStringsFromFile(Stream stream, string fileName, string contentType, IStringLocalizer localizer)
        {
            IDataExtractor extracter;
            if (contentType == MimeTypes.Csv || (fileName?.ToLower()?.EndsWith(".csv") ?? false))
            {
                extracter = new CsvExtractor();
            }
            else if (contentType == MimeTypes.Excel || (fileName?.ToLower()?.EndsWith(".xlsx") ?? false))
            {
                extracter = new ExcelExtractor();
            }
            else
            {
                throw new FormatException(localizer["Error_OnlyCsvOrExcelAreSupported"]);
            }

            // Extrat and return
            try
            {
                return extracter.Extract(stream).ToList();
            }
            catch (Exception ex)
            {
                // Report any errors during extraction
                string msg = localizer["Error_FailedToParseFileError0", ex.Message];
                throw new ServiceException(msg);
            }
        }

        /// <summary>
        /// Returns the E.164 representation of the phone number. Which starts with an optional '+' sign followed by digits only
        /// </summary>
        public static string ToE164(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return null;
            }

            // Normalize the phone number to E.164 format (Optional + sign, followed only by digits)
            var e164 = new System.Text.StringBuilder();

            // Start with a '+' sign if there is one
            if (phoneNumber.StartsWith('+'))
            {
                e164.Append('+');
            }

            // Then append only the digits
            foreach (var digit in phoneNumber.Where(char.IsDigit))
            {
                e164.Append(digit);
            }

            return e164.ToString();
        }

        /// <summary>
        /// Returns all new images in the saved entities with their blob names, after standardizing their size and format.
        /// Meanwhile for each entity with a new image the value of <see cref="EntityMetadata.FileId"/> on that entity is
        /// set to the file name (before applying the blob name function)
        /// </summary>
        public static IEnumerable<(string blobName, byte[] blobBytes)> ExtractImages<TEntity>(List<TEntity> entities, Func<string, string> blobNameFunc) where TEntity : Entity, IEntityWithImage
        {
            // Get new image Ids and bytes that should be added to blob storage
            foreach (var entity in entities)
            {
                byte[] imageBytes = entity.Image;
                if (imageBytes != null)
                {
                    if (imageBytes.Length == 0) // This means delete the image
                    {
                        if (entity.Id != 0)
                        {
                            entity.EntityMetadata.FileId = null;
                        }
                    }
                    else
                    {
                        // Specify that ImageId should be set to a new GUID
                        string imageId = Guid.NewGuid().ToString();
                        entity.EntityMetadata.FileId = imageId;

                        // Below we process the new image bytes
                        // We make the image smaller and turn it into JPEG
                        using (var image = Image.Load(imageBytes))
                        {
                            // Resize to 128x128px
                            image.Mutate(c => c.Resize(new ResizeOptions
                            {
                                // 'Max' mode maintains the aspect ratio and keeps the entire image
                                Mode = ResizeMode.Max,
                                Size = new Size(128),
                                Position = AnchorPositionMode.Center
                            }));

                            // Some image formats like PNG support transparent regions
                            // These regions will turn black in JPEG format unless we do this
                            image.Mutate(c => c.BackgroundColor(new Rgba32(255, 255, 255))); ;

                            // Save as JPEG
                            var memoryStream = new MemoryStream();
                            image.SaveAsJpeg(memoryStream);
                            imageBytes = memoryStream.ToArray();

                            // Note: JPEG is the format of choice for photography.
                            // It provides better quality at a lower size for real life photographs
                            // which is what most of these pictures are expected to be
                        }

                        // Add it to blobs to create
                        string blobName = blobNameFunc(imageId);
                        yield return (blobName, imageBytes);
                    }
                }
            }
        }

        /// <summary>
        /// Returns all new images in the saved entities with their blob names, after standardizing their size and format.
        /// Meanwhile for each entity with a new image the value of <see cref="EntityMetadata.FileId"/> on that entity is
        /// set to the file name (before applying the blob name function)
        /// </summary>
        public static IEnumerable<(string blobName, byte[] blobBytes)> ExtractAttachments<T>(List<T> entities, Func<T, IEnumerable<IAttachment>> attachmentsFunc, Func<string, string> blobNameFunc)
        {
            // The new attachments
            foreach (var entity in entities)
            {
                var attachments = attachmentsFunc(entity);
                if (attachments != null)
                {
                    foreach (var a in attachments)
                    {                    // New attachments
                        if (a.Id == 0 && a.File != null)
                        {
                            // Add extras: file Id and size
                            byte[] fileBytes = a.File;
                            string fileId = Guid.NewGuid().ToString();

                            // Set it in the attachment metadata
                            a.EntityMetadata.FileId = fileId;
                            a.EntityMetadata.FileSize = fileBytes.LongLength;

                            // Add to _blobsToSave
                            string blobName = blobNameFunc(fileId);
                            yield return (blobName, fileBytes);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the value of <see cref="ILocationEntityForSave.LocationWkb"/> according to to the value of <see cref="ILocationEntityForSave.LocationJson"/>.
        /// </summary>
        /// <remarks>
        /// GeoJSON library is hardcoded to work with Newtonsoft, nothing we can do about it.
        /// </remarks>
        public static void SynchronizeWkbWithJson<T>(T entity) where T : EntityWithKey, ILocationEntityForSave
        {
            // Here we convert the GeoJson to Well-Known Binary
            var json = entity.LocationJson;
            if (string.IsNullOrWhiteSpace(json))
            {
                entity.LocationWkb = null;
                return;
            }

            try
            {
                var spy = JsonConvert.DeserializeObject<GeoJsonSpy>(json);
                if (spy.Type == GeoJSONObjectType.Feature)
                {
                    // A simple feature can be turned in to a simple WKB
                    var feature = JsonConvert.DeserializeObject<Feature>(json);

                    var geometry = feature?.Geometry;
                    entity.LocationWkb = geometry?.ToWkb();
                }
                else if (spy.Type == GeoJSONObjectType.FeatureCollection)
                {
                    // A feature collection must be converted to a geometry collection and then turned to WKB
                    var coll = JsonConvert.DeserializeObject<FeatureCollection>(json);
                    var geometries = coll?.Features?.Select(feat => feat.Geometry)?.Where(e => e != null) ?? new List<IGeometryObject>();

                    if (geometries.Count() == 1)
                    {
                        // If it's just a single geometry, no need to wrap it in a geometry collection
                        var geometry = geometries.Single();
                        entity.LocationWkb = geometry?.ToWkb();
                    }
                    else
                    {
                        // If it's zero or multiple geometries, wrap in a geometry collection
                        var geomCollection = new GeometryCollection(geometries);
                        entity.LocationWkb = geomCollection?.ToWkb();
                    }
                }
                else
                {
                    // I don't know what'd be the point of localizing this message
                    throw new InvalidOperationException("Root GeoJSON element must be a feature or a feature collection");
                }
            }
            catch (Exception ex)
            {
                entity.EntityMetadata.LocationJsonParseError = ex.Message;
                return;
            }
        }

        /// <summary>
        /// Used to peek at the root element of a GeoJson string using JSON.NET.
        /// </summary>
        public class GeoJsonSpy : IGeometryObject
        {
            [JsonProperty(PropertyName = "type")]
            public GeoJSONObjectType Type { get; set; }
        }
    }
}
