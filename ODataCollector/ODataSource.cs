namespace Splunk
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;
   using System.Net;
   using Simple.OData.Client;
   using System.Collections;

   /// <summary>
   /// And OData Streaming Source
   /// </summary>
   public class ODataSource : IEnumerable<IDictionary<string, object>>
   {

      /// <summary>
      /// Gets or sets the URI address.
      /// </summary>
      /// <value>The address.</value>
      public string Address { get; set; }
      /// <summary>
      /// Gets or sets the name of the resource type.
      /// </summary>
      /// <value>The name of the resource.</value>
      public string Resource { get; set; }

      /// <summary>
      /// Gets or sets the query/filter.
      /// </summary>
      /// <value>The filter.</value>
      public string Filter { get; set; }

      /// <summary>
      /// Gets or sets the credentials.
      /// </summary>
      /// <value>The credentials.</value>
      public ICredentials Credentials { get; set; }

      //// NOTE: we could, hypothetically expose a way to: .OrderBy().OrderByDescending().OrderBy()
      ///// <summary>
      ///// Gets or sets the list of property names to order by.
      ///// </summary>
      ///// <value>The properties to order by.</value>
      //public string[] OrderBy { get; set; }

      ///// <summary>
      ///// Gets or sets a value indicating whether to order descending (defaults to ascending).
      ///// </summary>
      ///// <value><c>true</c> if the sort should be descending; otherwise, <c>false</c>.</value>
      //public bool OrderDescending { get; set; }

      ///// <summary>
      ///// Gets or sets a list of property values to select in the results
      ///// </summary>
      ///// <value>The property names.</value>
      //public string[] Select { get; set; }

      public IEnumerator<IDictionary<string, object>> GetEnumerator()
      {
         // we can't enumerate until we have, at least, the address and resource
         if (string.IsNullOrWhiteSpace(Address))
            throw new InvalidOperationException("You must set (at least) the Address before enumerating a data source.");

         if (!string.IsNullOrEmpty(Resource))
         {
            var settings = new ODataClientSettings(Address, Credentials);
            var client = new ODataClient(settings).For(Resource);

            if (!string.IsNullOrWhiteSpace(Filter))
               client = client.Filter(Filter);

            //if (Select != null && Select.Length > 0) // !string.IsNullOrWhiteSpace(Select))
            //   client = client.Select(Select);

            //if (OrderBy != null && OrderBy.Length > 0) // !string.IsNullOrWhiteSpace(OrderBy))
            //   client = OrderDescending ? client.OrderByDescending(OrderBy) : client = client.OrderBy(OrderBy);
            return client.FindEntries().GetEnumerator();
         }
         else
         {
            var uri = new Uri(Address);
            var resource = uri.Segments.Last();
            if(resource.EndsWith("/"))
               throw new InvalidOperationException("You must either set the Resource, or specify an Address which includes the resource and query string, like ../Packages.");

            var query = resource + uri.Query;
            var path = string.Join("", uri.Segments.Take(uri.Segments.Length - 1).ToArray());
            // var address = uri.Scheme + "://" + uri.DnsSafeHost + ":" + uri.Port + path;
            var address = uri.AbsoluteUri.Substring(0, uri.AbsoluteUri.Length - uri.PathAndQuery.Length) + path;

            var settings = new ODataClientSettings(address, Credentials);
            var client = new ODataClient(settings);

            return client.FindEntries(query).GetEnumerator();
         }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }
   }
}
