using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Graphs;
using Microsoft.Azure.Graphs.Elements;

namespace GraphGetStarted
{
    public class PersonRepository
        : IPersonRepository, IDisposable
    {
        private readonly string _endpoint;
        private readonly string _authKey;
        private readonly DocumentClient _client;
        private bool _isDisposed;
        private ResourceResponse<DocumentCollection> _graph;

        public PersonRepository()
        {
            _endpoint = ConfigurationManager.AppSettings["Endpoint"];
            _authKey = ConfigurationManager.AppSettings["AuthKey"];

            _client = new DocumentClient(
                new Uri(_endpoint),
                _authKey,
                new ConnectionPolicy {ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp});

            _graph =  _client.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri("graphdb"),
                    new DocumentCollection { Id = "Persons" },
                    new RequestOptions { OfferThroughput = 1000 })
                .Result;
        }

        public async Task AddAsync(Person person)
        {
            //This is kinda smelly. The Values aren't scrubbed for malicious content, and it should be more generic.
            var query =
                $"g.addV('person').property('id', '{person.Id}').property('firstName', '{person.Firstname}').property('lastName', '{person.Lastname}').property('age', {person.Age})";

            var q = _client.CreateGremlinQuery<dynamic>(_graph, query);

            await q.ExecuteNextAsync();
        }

        public async Task<Person> GetAsync(int id)
        {
            var query = $"g.V('{id}')";

            var q = _client.CreateGremlinQuery<Vertex>(_graph, query);

            var result = (await q.ExecuteNextAsync<Vertex>()).FirstOrDefault();

            if (result == null)
                return null;

            var person = new Person{Id = id};

            //is there an easier way to De-Serialize this, feel like we should be able to use a JsonConverter to do this directly
            foreach (var prop in result.GetVertexProperties())
            {
                switch (prop.Key.ToLowerInvariant())
                {
                    case "age":
                        person.Age = Convert.ToInt32(prop.Value);
                        break;
                    case "firstname":
                        person.Firstname = prop.Value.ToString();
                        break;
                    case "lastname":
                        person.Lastname = prop.Value.ToString();
                        break;
                }
            }

            return person;
        }

        public async Task DeleteAsync(int id)
        {
            var query = $"g.V('{id}').drop()";

            var q = _client.CreateGremlinQuery<dynamic>(_graph, query);

            await q.ExecuteNextAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                if (disposing)
                {
                    _client?.Dispose();
                    _graph = null;
                }
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PersonRepository()
        {
            Dispose(false);
        }
    }
}