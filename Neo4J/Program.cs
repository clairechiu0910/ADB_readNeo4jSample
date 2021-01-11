using Neo4j.Driver;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Neo4J
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (var greeter = new HelloWorldExample("neo4j://localhost:7687", "asd", "1234"))
            {
                //greeter.PrintGreeting("hello, world");
                greeter.QueryGreeting();
            }
        }

        public class DriverLifecycleExample : IDisposable
        {
            public IDriver Driver { get; }

            public DriverLifecycleExample(string uri, string user, string password)
            {
                Driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
            }

            public void Dispose()
            {
                Driver?.Dispose();
            }
        }




        public class HelloWorldExample : IDisposable
        {
            private readonly IDriver _driver;

            public HelloWorldExample(string uri, string user, string password)
            {
                _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
            }

            public void PrintGreeting(string message)
            {
                using (var session = _driver.Session())
                {
                    var greeting = session.WriteTransaction(tx =>
                    {
                        var result = tx.Run("CREATE (a:Greeting) " +
                                            "SET a.message = $message " +
                                            "RETURN a.message + ', from node ' + id(a)",
                            new { message });
                        return result.Single()[0].As<string>();
                    });
                    Console.WriteLine(greeting);
                }
            }
            public void QueryGreeting()
            {
                var statementText = "MATCH (n) RETURN n";
                using (var session = _driver.Session())
                {
                    var greeting = session.ReadTransaction(tx =>
                    {
                        var result = tx.Run(statementText);
                        foreach (var r in result)
                        {
                            //Get as an INode instance to access properties.
                            var node = r["n"].As<INode>();

                            //Properties are a Dictionary<string,object>, so you need to 'As' them
                            var message = node["message"].As<string>();

                            Console.WriteLine($"{message}");
                        }
                        return result;
                    });
                }
            }
            public void Dispose()
            {
                _driver?.Dispose();
            }
        }

    }
}
