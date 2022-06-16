using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using OpenXesNet.util;
using OpenXesNet.logging;
using OpenXesNet.extension.std;

namespace OpenXesNet.extension
{
    public class XExtensionManager
    {
        static XExtensionManager singleton = new XExtensionManager();
        public static XExtensionManager Instance
        {
            get { return singleton; }
        }
        readonly Dictionary<Uri, XExtension> extensionMap;
        readonly List<XExtension> extensionList;

        XExtensionManager()
        {
            this.extensionMap = new Dictionary<Uri, XExtension>();
            this.extensionList = new List<XExtension>();
            RegisterStandardExtensions();
        }

        public void Register(XExtension extension)
        {
            this.extensionMap.Add(extension.Uri, extension);

            int i = this.extensionList.IndexOf(extension);
            if (i < 0)
            {
                this.extensionList.Add(extension);
            }
            else
            {
                this.extensionList.Remove(extension);
                this.extensionList.Insert(i, extension);
            }
        }

        public XExtension GetByUri(Uri uri)
        {
            if(!this.extensionMap.ContainsKey(uri)){
                XLogging.Log(String.Format("Extension with uri '{0}' not regiesterd. Attempting download...", uri), XLogging.Importance.INFO);
                this.Register(XExtensionParser.Instance.Parse(uri));
            }
            XExtension extension = this.extensionMap[uri];

            return extension;
        }

        public XExtension GetByName(string name)
        {
            return this.extensionList.Find((XExtension ext) => ext.Name.Equals(name));
        }

        public XExtension GetByPrefix(string prefix)
        {
            return this.extensionList.Find((XExtension ext) => ext.Prefix.Equals(prefix));
        }

        public XExtension GetByIndex(int index)
        {
            if ((index < 0) || (index >= this.extensionList.Count))
            {
                return null;
            }
            return (this.extensionList[index]);
        }

        public int GetIndex(XExtension extension)
        {
            return this.extensionList.IndexOf(extension);
        }

        protected void RegisterStandardExtensions()
        {
            Register(XConceptExtension.Instance);
            Register(XCostExtension.Instance);
            Register(XIdentityExtension.Instance);
            Register(XLifecycleExtension.Instance);
            Register(XMicroExtension.Instance);
            Register(XOrganizationalExtension.Instance);
            Register(XSemanticExtension.Instance);
            Register(XSoftwareCommunicationExtension.Instance);
            Register(XSoftwareEventExtension.Instance);
            Register(XSoftwareTelemetryExtension.Instance);
            Register(XTimeExtension.Instance);
        }

        protected async System.Threading.Tasks.Task CacheExtensionAsync(Uri uri)
        {
            String uriStr = uri.ToString().ToLower();
            if (uriStr.EndsWith("/", StringComparison.Ordinal))
            {
                uriStr = uriStr.Substring(0, uriStr.Length - 1);
            }
            String fileName = uriStr.Substring(uriStr.LastIndexOf("/", StringComparison.Ordinal));
            if (!(fileName.EndsWith(".xesext", StringComparison.CurrentCulture)))
            {
                fileName = fileName + ".xesext";
            }
            FileStream cacheFile = File.Create(Path.Combine(XRuntimeUtils.GetExtensionCacheFolder(), fileName));
            Stream fs;
            try
            {
                HttpClient client = new HttpClient();
                fs = await client.GetStreamAsync(uri).ConfigureAwait(false);
                await fs.CopyToAsync(cacheFile);
                cacheFile.Flush();
            }
            catch (Exception e)
            {
                XLogging.Log(e.Message, XLogging.Importance.ERROR);
                throw e;
            }
        }

        protected void LoadExtensionCache()
        {
            long minModified = DateTime.Now.Ticks - 2592000000L;
            DirectoryInfo extFolder = Directory.CreateDirectory(XRuntimeUtils.GetExtensionCacheFolder());

            FileInfo[] extFiles = extFolder.GetFiles();
            if (extFiles == null)
            {
                XLogging.Log("Extension caching disabled (Could not access cache directory)!", XLogging.Importance.WARNING);

                return;
            }
            foreach (FileInfo extFile in extFiles)
            {
                if (!(extFile.FullName.ToLower().EndsWith(".xesext", StringComparison.Ordinal)))
                {
                    continue;
                }

                if (extFile.LastWriteTime.Ticks < minModified)
                {
                    extFile.Delete();
                }
                else
                    try
                    {
                        XExtension extension = XExtensionParser.Instance.Parse(extFile);
                        if (!(this.extensionMap.ContainsKey(extension.Uri)))
                        {
                            this.extensionMap.Add(extension.Uri, extension);
                            this.extensionList.Add(extension);
                            XLogging.Log("Loaded XES extension '" + extension.Uri + "' from cache",
                                        XLogging.Importance.DEBUG);
                        }
                        else
                        {
                            XLogging.Log("Skipping cached XES extension '" + extension.Uri + "' (already defined)",
                                        XLogging.Importance.DEBUG);
                        }

                    }
                    catch (Exception e)
                    {
                        XLogging.Log(e.Message, XLogging.Importance.ERROR);
                        throw e;
                    }
            }
        }
    }
}
