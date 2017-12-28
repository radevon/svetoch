using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace PumpVisualizer
{
    public class BundleConfig
    {

        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = true;

            bundles.Add(new ScriptBundle("~/unicomVisual").Include("~/js/angular/unicom/unicomVisual.js"));
            bundles.Add(new ScriptBundle("~/unicomArchive").Include("~/js/angular/unicom/unicomArchive.js"));
        }
    }
}