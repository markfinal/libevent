using Bam.Core;
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
                filter: new System.Text.RegularExpressions.Regex(@"^((?!.*arc4random)(?!.*devpoll)(?!.*epoll)(?!.*evport)(?!.*evthread_pthread)(?!.*kqueue)(?!.*poll)(?!.*select).*)$"));

            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (this.BuildEnvironment.Platform.Includes(EPlatform.Windows))
                    {
                        compiler.PreprocessorDefines.Add("WIN32");
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/compat"));
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/WIN32-Code/nmake"));
                    }

                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        vcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level2; // will not compile at a higher warning level
                    }
                });

            if (this.BuildEnvironment.Platform.Includes(EPlatform.Windows))
            {
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

            var generateConfig = Graph.Instance.FindReferencedModule<GenerateConfigHeader>();
            source.DependsOn(generateConfig);

            this.Requires(generateConfig); // this is for IDE projects, which require a different level of granularity

            var openSSLCopyStandardHeaders = Graph.Instance.FindReferencedModule<openssl.CopyStandardHeaders>();
            var openSSLConfigHeader = Graph.Instance.FindReferencedModule<openssl.GenerateConfHeader>();
            source.DependsOn(openSSLCopyStandardHeaders, openSSLConfigHeader);

            this.CompileAgainst<openssl.OpenSSL>(source);
        }
    }
}
