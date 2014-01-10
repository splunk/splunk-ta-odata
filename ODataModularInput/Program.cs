using System.Collections;

namespace Splunk.ModularInputs
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;
   using System.Threading.Tasks;
   using System.Net;
   using System.IO;
   using Microsoft.Data.OData;
   using Splunk;

   public class ModularInput : Script
   {
      public static int Main(string[] args)
      {
         return Run<ModularInput>(args);
      }

      public override Scheme Scheme
      {
         get
         {
            return new Scheme
            {
               Title = "OData Resource Monitor",
               Description = "Repeatedly query an OData resource, and collect new values",
               StreamingMode = StreamingMode.Xml,
               Endpoint =
               {
                  Arguments = new List<Argument>
                  {
                     new Argument {
                        Name = "name",
                        Description = "The stanza name must be unique"
                     },
                     new Argument {
                        Name = "address",
                        Description = "The uri for the OData web service",
                        DataType = DataType.String,
                        RequiredOnCreate = true,
                     },
                     new Argument{
                        Name = "resource",
                        Description = "The name of the type of resources to be retrieved",
                        DataType = DataType.String,
                        RequiredOnCreate = false,
                     },
                     new Argument {
                        Name = "filter",
                        Description = "The OData query as a filter string",
                        DataType = DataType.String,
                        RequiredOnCreate = false,
                     },
                     new Argument {
                        Name = "tailfilterproperty",
                        Description = "The name of property to use for tail filtering",
                        DataType = DataType.String,
                        RequiredOnCreate = false,
                     },
                     new Argument {
                        Name = "defaulttailfilter",
                        Description = "A default value for the tailfilterproperty",
                        DataType = DataType.String,
                        RequiredOnCreate = false,
                     },
                     new Argument {
                        Name = "includeEmpty",
                        Description = "Controls whether to output Keys with empty values",
                        DataType = DataType.Boolean,
                        RequiredOnCreate = false,
                     },
                  }
               }
            };
         }
      }

      /// <summary>
      /// Streams the events.
      /// </summary>
      /// <param name="inputDefinition">The input definition.</param>
      public override void StreamEvents(InputDefinition inputDefinition)
      {
         string tailFilters = inputDefinition.Stanza.SingleValueParameters.GetValueOrDefault("tailfilterproperty","");
         string filter = inputDefinition.Stanza.SingleValueParameters.GetValueOrDefault("filter", "");
         string address = inputDefinition.Stanza.SingleValueParameters["address"];
         string store = string.Empty;
         string last = string.Empty;

         if (!string.IsNullOrEmpty(tailFilters))
         {
            store = Path.Combine(inputDefinition.CheckpointDirectory, "lastvalue.txt");

            last = File.Exists(store) 
               ? File.ReadAllText(store) // .Split('\n')
               : inputDefinition.Stanza.SingleValueParameters.GetValueOrDefault("defaulttailfilter",""); //.Split('\n');

            if (!string.IsNullOrEmpty(filter))
            {
               filter = string.Format(filter, last);
            }
            else
            {
               address = string.Format(address, last);
            }

         }

         IEnumerable<IDictionary<string, object>> source = new Splunk.ODataSource()
         {
            Credentials = System.Net.CredentialCache.DefaultNetworkCredentials,
            Address = address,
            Resource = inputDefinition.Stanza.SingleValueParameters.GetValueOrDefault("resource",""),
            Filter = filter,
            //OrderBy = inputDefinition.Stanza.MultiValueParameterXmlElements,
            //Select = inputDefinition.Stanza.MultiValueParameterXmlElements,            
         };


         string includeEmptyString;
         bool includeEmpty = false;
         if(inputDefinition.Stanza.SingleValueParameters.TryGetValue("includeEmpty", out includeEmptyString)) {
            bool.TryParse(includeEmptyString, out includeEmpty);
         }
         using (var writer = new EventStreamWriter())
         {
            foreach (IDictionary<string, object> item in source)
            {
               writer.Write(new EventElement
               {
                  Data = item.ToString(includeEmpty: includeEmpty),
                  Stanza = inputDefinition.Stanza.Name,                  
               });

               if (!string.IsNullOrEmpty(tailFilters))
               {
                  last = ((IDictionary) item).SelectRecursive(tailFilters.Split(',')).ToStringInvariant();
               }
            }

            if (!string.IsNullOrEmpty(tailFilters))
            {
               using (var stream = File.Open(store, FileMode.Create, FileAccess.Write))
               {
                  using (var storeWriter = new StreamWriter(stream))
                  {
                     storeWriter.WriteLine(last);
                  }
               }
            }
         }
      }

      /// <summary>
      /// Performs validation for configurations of a new input being created.
      /// </summary>
      /// <param name="validationItems">Configuration data to validate.</param>
      /// <param name="errorMessage">Message to display in UI when validation
      /// fails.</param>
      /// <returns>A value indicating whether the validation
      /// succeeded.</returns>
      /// <remarks>An application can override this method to perform custom
      /// validation logic.</remarks>
      public override bool Validate(ValidationItems validationItems, out string errorMessage)
      {
         // TODO, process validationItems.Item.Parameters
         return base.Validate(validationItems, out errorMessage);
      }
   }
}
