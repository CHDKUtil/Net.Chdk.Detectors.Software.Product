﻿using Net.Chdk.Model.Card;
using Net.Chdk.Model.Software;
using Net.Chdk.Providers.Boot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Net.Chdk.Detectors.Software.Product
{
    public abstract class ProductDetector : IProductDetector
    {
        private IBootProvider BootProvider { get; }

        protected ProductDetector(IBootProviderResolver bootProviderResolver)
        {
            BootProvider = bootProviderResolver.GetBootProvider(CategoryName);
        }

        public SoftwareProductInfo GetProduct(CardInfo cardInfo)
        {
            var rootPath = cardInfo.GetRootPath();
            var productPath = Path.Combine(rootPath, ProductName);
            if (!Directory.Exists(productPath))
                return null;

            return new SoftwareProductInfo
            {
                Name = ProductName,
                Category = CategoryName,
                Version = GetVersion(rootPath),
                Created = GetCreationTime(cardInfo),
                Language = GetLanguage(rootPath),
            };
        }

        public abstract string CategoryName { get; }

        protected abstract string ProductName { get; }

        protected abstract Version GetVersion(string rootPath);

        protected abstract CultureInfo GetLanguage(string rootPath);

        protected static T GetValue<T>(string basePath, IDictionary<string, string> mapping, Func<string, T> getValue)
            where T : class
        {
            foreach (var kvp in mapping)
            {
                var filePath = Path.Combine(basePath, kvp.Key);
                if (File.Exists(filePath))
                    return getValue(kvp.Value);
            }
            return null;
        }

        private DateTime GetCreationTime(CardInfo cardInfo)
        {
            var rootPath = cardInfo.GetRootPath();
            var diskbootPath = Path.Combine(rootPath, BootProvider.FileName);
            return File.GetCreationTimeUtc(diskbootPath);
        }
    }
}
