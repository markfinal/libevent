using Bam.Core;
using System.Linq;
namespace libevent
{
    [ModuleGroup("Thirdparty/libevent")]
    sealed class libevent :
        C.StaticLibrary
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/include"));
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagebuilddir)"));
                    }
                });

            this.CreateHeaderContainer("$(packagedir)/*.h");
            // Note: some files not supported on Windows
            var source = this.CreateCSourceContainer("$(packagedir)/*.c",
                filter: new System.Text.RegularExpressions.Regex(@"^((?!.*arc4random)(?!.*devpoll)(?!.*epoll)(?!.*evport)(?!.*evthread_pthread)(?!.*kqueue)(?!.*poll)(?!.*select)(?!.*_iocp)(?!.*bufferevent_async)(?!.*win32).*)$"));

            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/compat"));
                    if (this.BuildEnvironment.Platform.Includes(EPlatform.Windows))
                    {
                        compiler.PreprocessorDefines.Add("WIN32");
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/WIN32-Code/nmake"));
                    }
                    else
                    {
                        var cCompiler = settings as C.ICOnlyCompilerSettings;
                        cCompiler.LanguageStandard = C.ELanguageStandard.C99; // references to 'inline'
                    }

                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        vcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level2; // will not compile at a higher warning level
                    }
                });

            if (this.BuildEnvironment.Platform.Includes(EPlatform.Windows))
            {
                source.AddFiles("$(packagedir)/buffer_iocp.c");
                source.AddFiles("$(packagedir)/bufferevent_async.c");
                source.AddFiles("$(packagedir)/event_iocp.c");
                source.AddFiles("$(packagedir)/evthread_win32.c");
                source.AddFiles("$(packagedir)/win32select.c");
                if (this.Librarian is VisualCCommon.Librarian)
                {
                    this.CompileAgainst<WindowsSDK.WindowsSDK>(source);
                }
                else
                {
                    source.PrivatePatch(settings =>
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.PreprocessorDefines.Add("EVENT__HAVE_STDINT_H"); // need to include stdint.h for Mingw
                        });
                }
            }
            else if (this.BuildEnvironment.Platform.Includes(EPlatform.Linux))
            {
                source.Children.Where(item => item.InputPath.Parse().Contains("bufferevent_ratelim.c")).ToList().ForEach(item =>
                    {
                        item.PrivatePatch(settings =>
                            {
                                var compiler = settings as C.ICommonCompilerSettings;
                                compiler.DisableWarnings.AddUnique("pointer-to-int-cast");
                                /*
                                libevent/bufferevent_ratelim.c:667:50: error: cast from pointer to integer of different size [-Werror=pointer-to-int-cast]
                                (ev_uint32_t) ((now.tv_sec + now.tv_usec) + (ev_intptr_t)g));
                                 */
                            });
                    });
            }

            var generateConfig = Graph.Instance.FindReferencedModule<GenerateConfigHeader>();
            var generatePrivateConfig = Graph.Instance.FindReferencedModule<GeneratePrivateConfigHeader>();
            source.DependsOn(generateConfig, generatePrivateConfig);

            this.Requires(generateConfig, generatePrivateConfig); // this is for IDE projects, which require a different level of granularity

            var openSSLCopyStandardHeaders = Graph.Instance.FindReferencedModule<openssl.CopyStandardHeaders>();
            var openSSLConfigHeader = Graph.Instance.FindReferencedModule<openssl.GenerateConfHeader>();
            source.DependsOn(openSSLCopyStandardHeaders, openSSLConfigHeader);

            this.CompileAgainst<openssl.OpenSSL>(source);
        }
    }
}
