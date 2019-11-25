using Capl.Authorization;
using Orleans;
using Orleans.Concurrency;
using Orleans.Providers;
using Piraeus.GrainInterfaces;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Piraeus.Grains
{
    [Reentrant]
    [StorageProvider(ProviderName = "store")]
    [Serializable]
    public class AccessControl : Grain<AccessControlState>, IAccessControl
    {
        public async Task ClearAsync()
        {
            await WriteStateAsync();
        }

        public async Task<AuthorizationPolicy> GetPolicyAsync()
        {
            AuthorizationPolicy policy = null;

            //serializing to byte array avoids issues with recursion serialization
            //when storage provider for grain state uses json serialization format.
            if (State.Policy != null)
            {
                using (MemoryStream stream = new MemoryStream(State.Policy))
                {
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        policy = AuthorizationPolicy.Load(reader);
                        reader.Close();
                    }
                }
            }

            return await Task.FromResult<AuthorizationPolicy>(policy);
        }

        public async Task UpsertPolicyAsync(AuthorizationPolicy policy)
        {
            //deserializing to byte array avoids issues with recursion deserialization
            //when storage provider for grain state uses json serialization format.

            XmlWriterSettings settings = new XmlWriterSettings() { OmitXmlDeclaration = true };
            StringBuilder builder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(builder, settings))
            {
                policy.WriteXml(writer);
                writer.Flush();
                writer.Close();
            }

            State.Policy = Encoding.UTF8.GetBytes(builder.ToString());
            await WriteStateAsync();
        }

        public override async Task OnDeactivateAsync()
        {
            await WriteStateAsync();
        }
    }
}
