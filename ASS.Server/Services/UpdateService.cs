using ASS.Server.Extensions;
using ASS.Server.Helpers;
using Emet.FileSystems;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ASS.Server.Services
{

    class UpdateService
    {

        private IConfiguration config;
        private IServiceProvider serviceProvider;
        private string realLivePath;
        private Repository repo;
        private ILogger logger;

        public UpdateService(IServiceProvider sp, IConfiguration configuration, ILogger<UpdateService> log)
        {
            config = configuration;
            serviceProvider = sp;
            logger = log;
        }

        public string GetRpositoryDirectory(params string[] extraPaths) => FileSystemHelper.GetPath(config, extraPaths, "Repo");
        public string GetOverrideDirectory(params string[] extraPaths) => FileSystemHelper.GetPath(config, extraPaths, "Override");
        public string GetLiveDirectory(params string[] extraPaths) => FileSystemHelper.GetPath(config, extraPaths, "Live");
        public string GetRealLiveDirectory(params string[] extraPaths)
        {
            if (string.IsNullOrEmpty(realLivePath))
                realLivePath = Path.GetFullPath(Path.Combine(GetLiveDirectory(), "..", FileSystem.ReadLink(GetLiveDirectory())));
            return Path.GetFullPath(Path.Combine(extraPaths.PreAppend(realLivePath)));
        }

        public string GetStagingDirectory(params string[] extraPaths) => FileSystemHelper.GetPath(config, extraPaths, getStagingName(Path.GetFileName(GetRealLiveDirectory())));
        private CredentialsHandler getCredentialsHandler() => serviceProvider.GetRequiredService<GitCredentialService>().GetCredentials;
        private Signature getSignature() => new Signature(config["Repository:Author:Name"] ?? "Aurora Server System", config["Repository:Author:Email"] ?? "ass@aurorastation.org", DateTimeOffset.Now);
        private string getStagingName(string liveRealName = null)
        {
            switch (liveRealName)
            {
                case "DM_A":
                    return "DM_B";
                case "DM_B":
                    return "DM_A";
                default:
                    return "DM_A";
            }
        }

        private void loadRepo()
        {
            if (repo != null)
                return;
            repo = new Repository(GetRpositoryDirectory());
        }

        public void InitilizeRepo()
        {
            var repoDir = GetRpositoryDirectory();
            if (Directory.Exists(repoDir))
                Directory.Delete(repoDir);
            var cloneOptions = new CloneOptions()
            {
                CredentialsProvider = getCredentialsHandler(),
                RecurseSubmodules = true,
            };
            if (!string.IsNullOrEmpty(config["Repository:Branch"]))
                cloneOptions.BranchName = config["Repository:Branch"];
            Repository.Clone(config["Repository:URL"], GetRpositoryDirectory(), cloneOptions);
        }

        public void UpdateRepo()
        {
            loadRepo();
            if (!(repo.Head?.IsTracking ?? false))
                throw new Exception("Cannot update while not on a tracked branch");
            Fetch();

            var trackedBranch = repo.Head.TrackedBranch;
            var originalCommit = repo.Head.Tip;
            if (repo.Head.Commits.Count(c => c.Sha == trackedBranch.Tip.Sha) != 0)
            {
                logger.LogInformation("Repository is up to date");
                return;
            }
            var mergeResult = repo.Merge(trackedBranch, getSignature());
            switch (mergeResult.Status)
            {
                case MergeStatus.FastForward:
                case MergeStatus.NonFastForward:
                    logger.LogInformation("Repository has been update successfully.");
                    break;
                case MergeStatus.Conflicts:
                    repo.Reset(ResetMode.Hard, originalCommit);
                    throw new Exception("Merge introduced merge conflicts, please reset and try again.");
                default:
                    break;
            }
            // TODO: Stage a update
        }

        public void CheckoutRepo(string branch = "master", string remote = "origin")
        {
            loadRepo();
            if (repo.Branches[branch] == null)
            {
                Fetch(remote);
                var trackedBranch = repo.Branches[$"{remote}/{branch}"];
                if (trackedBranch == null)
                    throw new Exception($"Branch '{branch}' is not found localy or on remote '{remote}'.");
                var newBranch = repo.CreateBranch(branch, trackedBranch.Tip);
                repo.Branches.Update(newBranch, b => b.TrackedBranch = trackedBranch.CanonicalName);
            }
            repo.Reset(ResetMode.Hard, repo.Head.TrackedBranch?.Tip ?? repo.Head.Tip);

            var checkoutOptions = new CheckoutOptions()
            {
                CheckoutModifiers = CheckoutModifiers.Force,
            };
            Commands.Checkout(repo, branch, checkoutOptions);
            //repo.Reset(ResetMode.Hard); // TGS has it, but do we need it?
            UpdateSubmodules();

            // TODO: Stage a update
        }

        public void Fetch(string remote = "origin")
        {
            loadRepo();
            Fetch(repo.Network.Remotes[remote]);
        }
        public void Fetch(Remote remote)
        {
            loadRepo();
            var fetchOptions = new FetchOptions()
            {
                Prune = true,
                CredentialsProvider = getCredentialsHandler()
            };
            Commands.Fetch(repo, remote.Name, remote.FetchRefSpecs.Select(X => X.Specification), fetchOptions, "");
        }

        public void UpdateSubmodules()
        {
            var submoduleUpdate = new SubmoduleUpdateOptions
            {
                CredentialsProvider = getCredentialsHandler(),
                Init = true
            };
            loadRepo();
            foreach (var module in repo.Submodules)
                repo.Submodules.Update(module.Name, submoduleUpdate);
        }
    }
}
