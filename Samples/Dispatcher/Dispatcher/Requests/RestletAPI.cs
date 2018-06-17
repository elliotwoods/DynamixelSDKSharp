using DynamixelSDKSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher.Requests
{
	class RestletAPI : IRequest
	{
		string hostName = "localhost";

		class APIExport
		{
			[DebuggerDisplay("type = {type}, name = {name}, path = {uri.path}")]
			public class Node
			{
				public enum Type
				{
					Project,
					Service,
					Request
				}

				public class Uri
				{
					public class Query
					{
						public string delimiter { get; set; } = "&";
						public List<string> items = new List<string>();
					}

					public class Scheme
					{
						public string name { get; set; } = "http";
						public string version { get; set; } = "V11";
					}

					public Query query { get; set; } = new Query();
					public Scheme scheme { get; set; } = new Scheme();
					public string host { get; set; }
					public string path { get; set; }
				}

				public class Method
				{
					public enum Type
					{
						POST,
						GET
					}

					public bool requestBody { get; set; } = false;
					public Type name { get; set; } = Type.GET;
				}

				public class Body
				{
					public string bodyType { get; set; } = "Text";
					public bool autoSetLength { get; set; } = true;
					public string textBody { get; set; }
				}

				public class Header
				{
					public bool enabled = true;
					public string name = "Content-Type";
					public string value = "application/json";
				};

				public string id { get; set; } = Guid.NewGuid().ToString();
				public DateTime lastModified { get; set; } = DateTime.Now;

				public Type type { get; set; }
				public string name { get; set; }
				public Uri uri { get; set; } = null;
				public Method method { get; set; } = null;

				public Body body { get; set; } = null;
				public string parentId { get; set; } = null;

				public List<Header> headers = null;
			}

			public List<Node> nodes = new List<Node>();
		}

		public object Perform()
		{
			var routes = AutoRouting.Routes;
			var apiExport = new APIExport();

			foreach(var route in routes)
			{
				var namespaceHeirarchy = route.Key.Split('/').ToList();
				var name = namespaceHeirarchy.Last();
				namespaceHeirarchy.RemoveAt(namespaceHeirarchy.Count - 1);

				string currentParentId = null;

				//check if we need to reference any parents
				{
					foreach (var serviceName in namespaceHeirarchy)
					{
						//search for service in nodes

						var matchingNodeQuery = apiExport.nodes.AsQueryable()
							.Where(x => x.parentId == currentParentId && x.name == serviceName);
						if(matchingNodeQuery.Count() >= 1)
						{
							//Service already exists
							var service = matchingNodeQuery.First();
							currentParentId = service.id;
						}
						else
						{
							//Create service
							var service = new APIExport.Node
							{
								type = APIExport.Node.Type.Service,
								name = serviceName,
								parentId = currentParentId
							};
							currentParentId = service.id;
							apiExport.nodes.Add(service);
						}
					}
				}

				//build the new node for this request
				{
					if(route.Value.RequestHandlerAttribute.Method.HasFlag(Method.GET))
					{
						var node = new APIExport.Node
						{
							type = APIExport.Node.Type.Request,
							name = name + " (GET)",
							uri = new APIExport.Node.Uri
							{
								host = String.Format("{0}:{1}", this.hostName, Program.Port),
								path = "/" + route.Key
							},
							method = new APIExport.Node.Method
							{
								name = APIExport.Node.Method.Type.GET
							},
							headers = new List<APIExport.Node.Header>
							{
								new APIExport.Node.Header()
							},
							parentId = currentParentId
						};
						apiExport.nodes.Add(node);
					}

					if (route.Value.RequestHandlerAttribute.Method.HasFlag(Method.POST))
					{
						var defaultRequestParameters = Activator.CreateInstance(route.Value.RequestType);

						var node = new APIExport.Node
						{
							type = APIExport.Node.Type.Request,
							name = name + " (POST)",
							uri = new APIExport.Node.Uri
							{
								host = String.Format("{0}:{1}", this.hostName, Program.Port),
								path = "/" + route.Key
							},
							method = new APIExport.Node.Method
							{
								name = APIExport.Node.Method.Type.POST,
								requestBody = true
							},
							body = new APIExport.Node.Body
							{
								textBody = JsonConvert.SerializeObject(defaultRequestParameters
									, Formatting.Indented
									, ProductDatabase.JsonSerializerSettings)
							},
							headers = new List<APIExport.Node.Header>
							{
								new APIExport.Node.Header()
							},
							parentId = currentParentId
						};
						apiExport.nodes.Add(node);
					}
				}
			}

			return apiExport;
		}
	}
}
