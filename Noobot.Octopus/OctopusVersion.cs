using System.Linq;
using Octopus.Client;
using Octopus.Client.Model;
using System;
using System.Collections.Generic;
using static Octopus.Client.Model.TenantVariableResource;

namespace Noobot.Octopus
{
    class OctopusVersion
    {
        private OctopusServerEndpoint endpoint;
        private OctopusRepository repository;

        public OctopusVersion(string octopusUrl, string octopusApiKey)
        {
            endpoint = new OctopusServerEndpoint(octopusUrl, octopusApiKey);
            repository = new OctopusRepository(endpoint);
        }
        public IEnumerable<string> Get(string argument)
        {
            InitializeCollections();
            var tenantName = GetTenantName(argument);
            if (String.IsNullOrEmpty(tenantName))
            {
                var env = GetEnvironmentName(argument);
                if (String.IsNullOrEmpty(env))
                    return WriteAllProjectDeployments();
                else
                    return WriteDeploymentsForEnvironment(env);
            }
            else
                return WriteLastProjectDeploymentForTenant(GetTenantDisplayName(tenantName), tenantName);
        }

        private string GetTenantDisplayName(string tenantName)
        {
            return tenantName.Replace("-tenant", "");
        }
        private IEnumerable<string> WriteDeploymentsForEnvironment(string env)
        {
            yield return("Deployments on " + env);
            foreach( ProjectResource p in repository.Projects.FindAll())
            {
                foreach (var dr in GetProjectLastDeploymentsByProjectId(p.Id).Where(d => GetEnvironmentById(d.EnvironmentId).Name == env))
                {
                    yield return($"   {p.Name,-30} - {GetReleaseById(dr.ReleaseId)?.Version,-30}");
                }
            };
        }
        private string GetEnvironmentName(string argument)
        {
            switch (argument.ToLower())
            {
                case "intacp":
                case "ia":
                case "acp1": return "Interne Acceptatie";
                case "extacp":
                case "ea":
                case "acp": return "Externe Acceptatie";
                case "prod":
                case "prd":
                case "pd": return "Productie";
                default: return "";
            }
        }
        private IEnumerable<string> WriteAllProjectDeployments()
        {
            foreach(ProjectResource p in repository.Projects.FindAll())
            {
                foreach( string s in WriteLastProjectDeployment(p))
                    yield return s;
            }

        }

        private string GetTenantName(string tenantName)
        {
            switch (tenantName.ToLower())
            {
                case "npo1":
                case "npo2":
                case "npo3": return tenantName.ToUpper() + "-tenant";
                case "intdev1":
                case "1":
                    return "InternDevelopment1-tenant";
                case "intdev2":
                case "2":
                    return "InternDevelopment2-tenant";
                case "intdev3":
                case "3":
                    return "InternDevelopment3-tenant";
                case "intdev4":
                case "4":
                    return "InternDevelopment4-tenant";
                case "intdev5":
                case "5":
                    return "InternDevelopment5-tenant";
                default: return "";
            }
        }

        private void InitializeCollections()
        {
            GetDeployments();
            GetReleases();
            GetEnvironments();
            GetTasks();
        }

        private IEnumerable<string> WriteLastProjectDeployment(ProjectResource p)
        {
            yield return p.Name;
            foreach (var dr in GetProjectLastDeploymentsByProjectId(p.Id))
            {
                yield return $"   {GetEnvironmentById(dr.EnvironmentId).Name,-25} - {GetTenantById(dr.TenantId)?.Name,-25} - {GetReleaseById(dr.ReleaseId)?.Version,-30}";
            }
        }

        private IEnumerable<string> WriteLastProjectDeploymentForTenant(string displayName, string tenantName)
        {
            yield return("Deployments on " + displayName);
            string tenantId = GetTenantByName(tenantName).Id;
            foreach( ProjectResource p in repository.Projects.FindAll())
            {
                foreach (var dr in GetProjectLastDeploymentsByProjectId(p.Id).Where(d => d.TenantId == tenantId && GetEnvironmentById(d.EnvironmentId).SortOrder == 0))
                {
                    yield return($"   {p.Name,-30} - {GetReleaseById(dr.ReleaseId).Version,-30}");
                }
            };
        }

        private ReleaseResource GetReleaseById(string releaseId)
        {
            return GetReleases().Single(e => e.Id == releaseId);
        }

        private TenantResource GetTenantById(string tenantId)
        {
            return GetTenants().SingleOrDefault(e => e.Id == tenantId);
        }

        private TenantResource GetTenantByName(string tenantName)
        {
            return GetTenants().SingleOrDefault(e => e.Name == tenantName);
        }

        private IEnumerable<ReleaseResource> _releases;
        private IEnumerable<ReleaseResource> GetReleases()
        {
            return _releases = _releases ?? repository.Releases.FindAll();
        }

        private IEnumerable<TenantResource> _tenants;
        private IEnumerable<TenantResource> GetTenants()
        {
            return _tenants = _tenants ?? repository.Tenants.FindAll();
        }

        private IEnumerable<DeploymentResource> _deployments;
        private IEnumerable<DeploymentResource> GetDeployments()
        {
            return _deployments = _deployments ?? repository.Deployments.FindAll();
        }

        private EnvironmentResource GetEnvironmentById(string environmentId)
        {
            return GetEnvironments().Single(e => e.Id == environmentId);
        }

        private IEnumerable<EnvironmentResource> _environments = null;
        public IEnumerable<EnvironmentResource> GetEnvironments()
        {
            return _environments = _environments ?? repository.Environments.GetAll();
        }

        public IEnumerable<DeploymentResource> GetProjectDeploymentsByProjectId(string projectId)
        {
            return GetDeployments().Where(x => x.ProjectId == projectId);
        }

        private IEnumerable<TaskResource> _tasks = null;
        public IEnumerable<TaskResource> GetTasks()
        {
            return _tasks = _tasks ?? repository.Tasks.FindAll();
        }

        public IEnumerable<DeploymentResource> GetProjectLastDeploymentsByProjectId(string projectId)
        {
            // Get all the deployments
            var deployments = GetProjectDeploymentsByProjectId(projectId);
            // Filter them by deployments that are not currently executing
            var completedDeployments = (
                from deployment in deployments
                let task = GetTasks().Single(t => t.Id == deployment.TaskId)
                where task.State != TaskState.Executing && deployment.ProjectId == projectId
                select deployment).ToList();
            var groupedDeployments = completedDeployments
                 .GroupBy(td => new { td.EnvironmentId, td.TenantId });
            var lastDeploymentsPerTenant = groupedDeployments
                 .Select(grp => grp.OrderByDescending(o => o.Created).First())
                 .ToList();
            return lastDeploymentsPerTenant
                .OrderBy(ld => GetEnvironmentById(ld.EnvironmentId).SortOrder)
                .ThenBy(ld => GetTenantById(ld.TenantId)?.Name);
        }
    }
}
