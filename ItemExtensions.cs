using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Publishing;

namespace Sitecore.Extensions
{
    public static class ItemExtensions
    {
        #region Identity Tests --------------------------------------------------------------------

        public static bool IsStandardValues(this Item item)
        {
            if (item == null)
                return false;
            bool isStandardValue = false;

            if (item.Template.StandardValues != null)
                isStandardValue = (item.Template.StandardValues.ID == item.ID);

            return isStandardValue;
        }

        public static bool IsTemplate(this Item item)
        {
            return item.Database.Engines.TemplateEngine.IsTemplatePart(item);
        }

        #endregion Identity Tests

        #region Templates -------------------------------------------------------------------------

        public static bool IsDerivedFromTemplate(this Item item, ID templateId)
        {
            if (item == null)
                return false;

            if (templateId.IsNull)
                return false;

            TemplateItem templateItem = item.Database.Templates[templateId];

            bool returnValue = false;
            if (templateItem != null)
            {
                Template template = TemplateManager.GetTemplate(item);

                returnValue = template != null && template.ID == templateItem.ID || template.DescendsFrom(templateItem.ID);
            }

            return returnValue;
        }

        public static IEnumerable<Item> ChildrenDerivedFrom(this Item item, ID templateId)
        {
            ChildList children = item.GetChildren();
            List<Item> childrenDerivedFrom = new List<Item>();

            foreach (Item child in children)
            {
                if (child.IsDerivedFromTemplate(templateId))
                    childrenDerivedFrom.Add(child);
            }

            return childrenDerivedFrom;
        }

        #endregion Templates

        #region Relations -------------------------------------------------------------------------

        public static IEnumerable<Item> GetReferrersAsItems(this Item item)
        {
            var links = Globals.LinkDatabase.GetReferrers(item);
            return links.Select(i => i.GetTargetItem()).Where(i => i != null);
        }

        public static IEnumerable<Item> GetAncestors(this Item item, Boolean descending = true)
        {
            return item.Axes.GetAncestors();
        }

        #endregion


        #region Publishing

        public static void Publish(this Item item, bool deep)
        {
            var publishOptions = new PublishOptions(item.Database,
                                                    Database.GetDatabase("web"),
                                                    PublishMode.SingleItem,
                                                    item.Language,
                                                    DateTime.Now);
            var publisher = new Publisher(publishOptions);
            publisher.Options.RootItem = item;
            publisher.Options.Deep = deep;
            publisher.Publish();
        }

        public static void UnPublish(this Item item)
        {
            item.Publishing.UnpublishDate = DateTime.Now;
            item.Publish(true);
        }

        public static void DeleteAndPublish(this Item item)
        {
            var parent = item.Parent;
            item.Delete();
            parent.Publish(true);
        }

        public static bool IsPublished(this Item pItem)
        {
            Database lWebDb = Factory.GetDatabase("web");
            if (pItem != null && lWebDb != null)
            {
                Item lWebItem = lWebDb.GetItem(pItem.ID, pItem.Language, pItem.Version);
                if (lWebItem == null || pItem.Statistics.Updated > lWebItem.Statistics.Updated)
                {
                    return false;
                }
            }
            return true;
        }

        public static Boolean IsValidForPublish(this Item item)
        {
            if (item == null)
                return false;


            CheckboxField hideVersion = item.Fields["__Hide version"];
            if (hideVersion.Checked)
                return false;


            DateTime validFrom = !String.IsNullOrEmpty(item["__Valid from"]) ? DateUtil.IsoDateToDateTime(item["__Valid from"]) : DateTime.MinValue;
            DateTime validTo = !String.IsNullOrEmpty(item["__Valid to"]) ? DateUtil.IsoDateToDateTime(item["__Valid to"]) : DateTime.MaxValue;


            return DateTime.Now >= validFrom && DateTime.Now <= validTo;
        }

        #endregion Publishing

        #region Languages -------------------------------------------------------------------------

        public static bool HasLanguage(this Item item, string languageName)
        {
            return ItemManager.GetVersions(item, LanguageManager.GetLanguage(languageName)).Count > 0;
        }

        public static bool HasLanguage(this Item item, Language language)
        {
            return ItemManager.GetVersions(item, language).Count > 0;
        }

        public static int LanguageVersionCount(this Item item, Language lang)
        {
            if (item == null)
                return 0;
            Item currentItem = item.Database.GetItem(item.ID, lang);
            return currentItem.Versions.Count > 0 ? currentItem.Versions.Count : 0;
        }

        #endregion Languages


        public static Boolean IsInContextSite(this Item item)
        {
            if (item == null)
                return false;

            Assert.IsNotNull(Context.Site, "Sitecore.Context.Site required by the Item.IsInSite extension is null");
            Assert.IsNotNullOrEmpty(Context.Site.RootPath, "Sitecore.Context.Site.RootPath required by the Item.IsInSite extension is null or empty");
            Assert.IsNotNull(Context.Database, "Sitecore.Context.Database required by the Item.IsInSite extension is null");

            Item rootItem = Context.Site.Database.Items[Context.Site.RootPath];
            Assert.IsNotNull(rootItem, String.Format("Unable to retrieve the item at path {0} using the database {1}", Context.Site.RootPath, Context.Database.Name));

            Item currentItem = item;

            while (currentItem != null)
            {
                if (currentItem.ID.Guid.Equals(rootItem.ID.Guid))
                    return true;

                currentItem = currentItem.Parent;
            }

            return false;
        }

    }
}