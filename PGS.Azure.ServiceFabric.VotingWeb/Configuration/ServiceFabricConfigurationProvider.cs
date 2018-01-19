using System.Fabric;
using System.Fabric.Description;
using Microsoft.Extensions.Configuration;
using ConfigurationSection = System.Fabric.Description.ConfigurationSection;

namespace PGS.Azure.ServiceFabric.VotingWeb.Configuration
{
    public class ServiceFabricConfigurationProvider : ConfigurationProvider
    {
        private readonly string _packageName;
        private readonly CodePackageActivationContext _context;

        public ServiceFabricConfigurationProvider(string packageName)
        {
            _packageName = packageName;
            _context = FabricRuntime.GetActivationContext();
            _context.ConfigurationPackageModifiedEvent += (sender, args) =>
            {
                LoadPackage(args.NewPackage, true);
                OnReload();
            };
        }

        public override void Load() => LoadPackage(_context.GetConfigurationPackageObject(_packageName), false);

        private void LoadPackage(ConfigurationPackage configurationPackage, bool reload)
        {
            if (reload)
            {
                Data.Clear();                
            }

            foreach (ConfigurationSection section in configurationPackage.Settings.Sections)
            {
                foreach (ConfigurationProperty configurationProperty in section.Parameters)
                {
                    Data[$"{section.Name}:{configurationProperty.Name}"] =
                        configurationProperty.IsEncrypted ? configurationProperty.DecryptValue().ToString() : configurationProperty.Value;
                }
            }
        }
    }
}