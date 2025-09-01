
using DeepSigma.OperatingSystem.Models;

namespace DeepSigma.OperatingSystem
{
    public static class AppInstaller
    {
        public static void Install(string installation_directory, ApplicationVersion current_version, string app_name, string source_directory, string target_install_directory, string cli_source_directory, bool auto = true)
        {
            ApplicationVersion? latest_version = AppVersioningService.GetLatestApplicationVersionFromDirectory(installation_directory);

            if(latest_version is null) return;
            if(latest_version.IsGreaterThan(current_version) == false) return;

            Terminal.RunCommand("AppInstaller", $"--app={app_name} --source={source_directory} --target={target_install_directory} --clisource={cli_source_directory} --auto={auto}");
        }

    }
}
