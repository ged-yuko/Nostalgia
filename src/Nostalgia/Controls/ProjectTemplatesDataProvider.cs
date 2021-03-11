using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Dialogs;
using Microsoft.VisualStudio.ExtensionsExplorer;
using Microsoft.VisualStudio.NewProjectDialog;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateProviders.Contracts;
using Microsoft.VisualStudio.TemplateProviders.TemplateProviderApi;
using Microsoft.VisualStudio.TemplateProviders.Templates;
using Microsoft.VisualStudio.TemplateProviders.Templates.MRU;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Expression = System.Linq.Expressions.Expression;

namespace Nostalgia.Controls
{
    public class MRUTemplateInfo2 : MRUTemplateInfo
    {
        public MRUTemplateInfo2()
        {
        }

        public MRUTemplateInfo2(MRUTemplateInfo2 mruTemplateInfo) : base(mruTemplateInfo)
        {
            this.IsPinned = mruTemplateInfo.IsPinned;
            this.LastAccessed = mruTemplateInfo.LastAccessed;
        }

        public bool IsPinned { get; set; }
        public DateTimeOffset LastAccessed { get; set; }
    }

    public class ProjectTemplate : DependencyObject
    {
        private MRUTemplateInfo2 _info;
        private IVsTemplate _vsTemplate;

        public ImageSource Icon { get; }

        public string Name { get { return _vsTemplate?.Name ?? _info.Name; } }
        public string ProjectType { get { return _vsTemplate?.ProjectType ?? _info.ProjectType; } }

        public DateTimeOffset LastAccessed { get { return _info.LastAccessed; } }

        public DelegateCommand Command { get; }

        #region bool IsPinned 

        public bool IsPinned
        {
            get { return (bool)GetValue(IsPinnedProperty); }
            set { SetValue(IsPinnedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPinned.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPinnedProperty =
            DependencyProperty.Register("IsPinned", typeof(bool), typeof(ProjectTemplate), new UIPropertyMetadata(default(bool)));

        #endregion

        public ProjectTemplate(MRUTemplateInfo2 info, IVsTemplate vsTemplateOrNull)
        {
            _info = info;
            _vsTemplate = vsTemplateOrNull;

            this.Icon = vsTemplateOrNull?.MediumThumbnailImage; // ?? new BitmapImage(new Uri("/Nostalgia;Icons/NewFileCollection/NewFileCollection_16x.png", UriKind.Relative));

            this.IsPinned = info.IsPinned;
            this.Command = new DelegateCommand(this.DoCreateProject);
        }

        private async void DoCreateProject()
        {
            _callNewProjectDialog.Value(_info.Hierarchy, _info.Name);
        }

        static Lazy<Action<string, string>> _callNewProjectDialog = new Lazy<Action<string, string>>(() => MakeNewProjectDialogMethod());

        static Action<string, string> MakeNewProjectDialogMethod()
        {
            var newProjInfoType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypesOrNone()).FirstOrDefault(t => t.Name == "VSNEWPROJECTDLGINFO");

            var sDialogService = Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider.GetService(typeof(SVsDialogService));
            var rawDialogService = (SVsDialogService)sDialogService;

            var hierarchyArg = Expression.Parameter(typeof(string), "hierarchy");
            var nameArg = Expression.Parameter(typeof(string), "name");

            var newProjInfoVar = Expression.Variable(newProjInfoType, "newProjInfo");
            var locVar = Expression.Variable(typeof(string), "locVar");
            var expr = Expression.Lambda<Action<string,string>>(
                Expression.Block(
                    new[] { newProjInfoVar, locVar },
                    Expression.Assign(newProjInfoVar, Expression.New(newProjInfoType)),
                    Expression.Assign(Expression.Field(newProjInfoVar, newProjInfoType.GetField("pwzExpand")), hierarchyArg),
                    Expression.Assign(Expression.Field(newProjInfoVar, newProjInfoType.GetField("pwzSelect")), nameArg),
                    Expression.Call(Expression.Constant(rawDialogService, typeof(SVsDialogService)), rawDialogService.GetType().GetMethod("InvokeDialog"), newProjInfoVar, locVar)
                ),
                new[] { hierarchyArg, nameArg }
            );

            var compiledExpr = expr.Compile();
            return compiledExpr;
        }

        //private async void DoCreateProjectOld()
        //{
        //    var newProjInfoType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypesOrNone()).FirstOrDefault(t => t.Name == "VSNEWPROJECTDLGINFO");
        //    var newProjInfo = Activator.CreateInstance(newProjInfoType);
        //    newProjInfoType.GetField("pwzExpand").SetValue(newProjInfo, _info.Hierarchy);
        //    newProjInfoType.GetField("pwzSelect").SetValue(newProjInfo, _info.Name);

        //    var sDialogService = Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider.GetService(typeof(SVsDialogService));
        //    var rawDialogService = (SVsDialogService)sDialogService;

        //    string loc = null;
        //    rawDialogService.GetType().GetMethod("InvokeDialog").Invoke(rawDialogService, new[] { newProjInfo, loc });
        //}
    }

    public class ProjectTemplatesDataProvider : DataSourceProviderBase
    {
        public ProjectTemplatesDataProvider()
        {

        }

        protected override object ObtainDataImpl()
        {
            // TODO seems some resources need to be disposed


            // ITemplateProvider

            // IVsDialogService, IVsDialogService2, IVsDialogService3, IVsTemplateProviderFactory
            var sDialogService = Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider.GetService(typeof(SVsDialogService));
            var rawDialogService = (SVsDialogService)sDialogService;
            //var vsTemplateProviderFactory = (IVsTemplateProviderFactory)sDialogService;
            //var vsDialogService2 = (Microsoft.VisualStudio.Shell.Interop.IVsDialogService2)sDialogService;
            var vsTemplateProvider = rawDialogService.GetInstalledTemplateProvider(true);
            var extTree = vsTemplateProvider.ExtensionsTree;
            // var tpl = extTree.Nodes[0].Extensions[0] as IVsTemplate;

            IEnumerable<object> ExpandTemplates(IVsExtensionsTreeNode root)
            {
                var stack = new Stack<IVsExtensionsTreeNode>();
                stack.Push(root);

                while (stack.Count > 0)
                {
                    var node = stack.Pop();

                    foreach (var item in node.Nodes)
                        stack.Push(item);

                    foreach (var item in node.Extensions)
                        yield return item;
                }
            }

            var allTemplates = ExpandTemplates(extTree).OfType<IVsTemplate>().ToList();

            //var dte = (DTE2)await ServiceProvider.GetGlobalServiceAsync(typeof(DTE));
            //EnvDTE90.Templates

            // var _settingsManager = ((await this._asyncServiceProvider.GetServiceAsync(typeof(Microsoft.Internal.VisualStudio.Shell.Interop.SVsSettingsPersistenceManager))) as ISettingsManager);
            var settingsManager = Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider.GetService(typeof(SVsSettingsPersistenceManager)) as ISettingsManager;

            var result = new List<ProjectTemplate>();

            var templatesList = settingsManager.GetOrCreateList("VS.IDE.Platform.ProjectTemplateMru", false);
            // var filtersList = settingsManager.GetOrCreateList("VS.IDE.Platform.ProjectTemplateFilters", false);

            foreach (string key in templatesList.Keys)
            {
                var templateInfo = templatesList.GetValueOrDefault<MRUTemplateInfo2>(key, null);

                // for an unknown reason template ids are messy and are unable to match sometimes, like for Core Console App template
                IVsTemplate vsTemplate = allTemplates.FirstOrDefault(t => !string.IsNullOrEmpty(t.NormalizedId()) && t.NormalizedId().Equals(templateInfo.Id, StringComparison.OrdinalIgnoreCase));
                // if (vsTemplate != null)
                // {
                result.Add(new ProjectTemplate(templateInfo, vsTemplate));
                // }
            }

            result.Sort((a, b) => b.LastAccessed.CompareTo(a.LastAccessed));

            return result;
        }

        [Guid("9b164e40-c3a2-4363-9bc5-eb4039def653")]
        private class SVsSettingsPersistenceManager
        {
        }

    }
}
