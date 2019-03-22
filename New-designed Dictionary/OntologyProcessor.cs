using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing;

namespace New_designed_Dictionary
{
    public class OntologyProcessor
    {
        public static object GetIndividualQueryResults(string individualQuery)
        {
            SparqlQueryParser parser = new SparqlQueryParser();
            Graph g = new Graph();
            TurtleParser rdfparser = new TurtleParser();
            rdfparser.Load(g, Resources.Paths.Ontology_Path);
            ISparqlDataset ds = new InMemoryDataset(g);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(ds);

            SparqlQuery sq = parser.ParseFromString(Resources.Queries.PREFIXES + individualQuery);
            return processor.ProcessQuery(sq);
        }
        public static List<string> GetIndividuals(string individualQuery)
        {
            List<string> individuals = new List<string>();
            object results = GetIndividualQueryResults(individualQuery);

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    foreach (var v in r)
                    {
                        individuals.Add(v.Value.ToString().Replace(Resources.Paths.Ontology_Base, "").Replace("'", "").Replace("_", " "));
                    }
                }
            }
            return individuals;
        }

        public static List<Tag> GetClassTags(List<string> tags, bool needsAll)
        {
            List<Tag> spheresOfUsage = new List<Tag>();

            foreach (var item in tags)
            {
                if (needsAll == false)
                {
                    if (item != "All tags")
                    {
                        spheresOfUsage.Add(new Tag { Name = item });
                    }
                }
                else
                {
                    spheresOfUsage.Add(new Tag { Name = item });
                }
            }

            return spheresOfUsage;
        }
        public static List<PartsOfSpeech> GetPartsOfSpeech()
        {
            List<PartsOfSpeech> PartsOfSpeech = new List<PartsOfSpeech>();

            Graph g = new Graph();
            TurtleParser parser = new TurtleParser();
            parser.Load(g, New_designed_Dictionary.Resources.Paths.Ontology_Path);

            IUriNode PartOfSpeech = g.CreateUriNode(new Uri(Resources.Paths.Ontology_PartOfSpeech));
            IEnumerable<Triple> ts = g.GetTriples(PartOfSpeech);
            ts = g.GetTriplesWithObject(PartOfSpeech);
            foreach (Triple t in ts)
            {
                PartsOfSpeech PoS = new PartsOfSpeech
                {
                    Name = t.Subject.ToString().Split('#')[t.Subject.ToString().Split('#').Length - 1]
                };
                PartsOfSpeech.Add(PoS);
            }

            return PartsOfSpeech;
        }

        private static Graph GetLoadedGraphWithTurtle(string filepath)
        {
            Graph g = new Graph();
            TurtleParser parser = new TurtleParser();
            parser.Load(g, filepath);
            return g;
        }
        
        public static void UpdateGraph(string subjectIndiv, string predicate, string objectIndiv, string pairPredicate = "")
        {
            Graph g = GetLoadedGraphWithTurtle(Resources.Paths.Ontology_Path);
            TurtleWriter turtleWriter = new TurtleWriter();


            IUriNode subject = g.CreateUriNode(UriFactory.Create(Resources.Paths.Ontology_Base + subjectIndiv.Replace(" ", "_")));
            IUriNode Predicate = g.CreateUriNode(UriFactory.Create(Resources.Paths.Ontology_Base + predicate));
            IUriNode objectToAssert = g.CreateUriNode(UriFactory.Create(Resources.Paths.Ontology_Base + objectIndiv.Replace(" ", "_")));
            if (pairPredicate != "")
            {
                IUriNode PairPredicate = g.CreateUriNode(UriFactory.Create(Resources.Paths.Ontology_Base + pairPredicate));
                g.Assert(new Triple(objectToAssert, PairPredicate, subject));
            }

            IEnumerable<Triple> WordSources = g.GetTriplesWithSubjectPredicate(subject, Predicate);
            foreach (Triple ts in WordSources)
            {
                IUriNode objectToRetract = g.CreateUriNode(UriFactory.Create(ts.Object.ToString()));
                if (objectToAssert != objectToRetract)
                {
                    IEnumerable<Triple> SourceWords = g.GetTriplesWithSubjectPredicate(subject, Predicate);
                    foreach (Triple tw in SourceWords)
                    {
                        IUriNode subjectToRetract = g.CreateUriNode(UriFactory.Create(tw.Subject.ToString()));
                        if (objectToAssert != subjectToRetract)
                        {
                            IUriNode PairPredicate = g.CreateUriNode(UriFactory.Create(Resources.Paths.Ontology_Base + pairPredicate));
                            g.Retract(new Triple(objectToRetract, PairPredicate, subjectToRetract));
                            break;
                        }
                    }
                    g.Retract(new Triple(subject, Predicate, objectToRetract));
                    break;
                }
            }

            g.Assert(new Triple(subject, Predicate, objectToAssert));


            turtleWriter.Save(g, @"D:\Folders\SampleSave.owl");
        }

        public static void AddGraph(string subjectIndiv, string predicate, string objectIndiv, string pairPredicate = "")
        {
            Graph g = GetLoadedGraphWithTurtle(Resources.Paths.Ontology_Path);
            TurtleWriter turtleWriter = new TurtleWriter();


            IUriNode subject = g.CreateUriNode(UriFactory.Create(Resources.Paths.Ontology_Base + subjectIndiv.Replace(" ", "_")));
            IUriNode Predicate = g.CreateUriNode(UriFactory.Create(Resources.Paths.Ontology_Base + predicate));
            IUriNode objectToAssert = g.CreateUriNode(UriFactory.Create(Resources.Paths.Ontology_Base + objectIndiv.Replace(" ", "_")));
            if (pairPredicate != "")
            {
                IUriNode PairPredicate = g.CreateUriNode(UriFactory.Create(Resources.Paths.Ontology_Base + pairPredicate));
                g.Assert(new Triple(objectToAssert, PairPredicate, subject));
            }
            
            g.Assert(new Triple(subject, Predicate, objectToAssert));


            turtleWriter.Save(g, @"D:\Folders\SampleSave.owl");
        }

        public static void AddIndividual(string value, string type)
        {
            Graph g = GetLoadedGraphWithTurtle(Resources.Paths.Ontology_Path);
            TurtleWriter turtleWriter = new TurtleWriter();

            IUriNode instanceNode = g.CreateUriNode(UriFactory.Create(Resources.Paths.Ontology_Base + value));
            IUriNode typeNode = g.CreateUriNode(UriFactory.Create(Resources.Paths.Ontology_Base + type));
            Individual individual = new Individual(instanceNode, typeNode, g);

            turtleWriter.Save(g, @"D:\Folders\SampleSave.owl");
        }

    }
}
