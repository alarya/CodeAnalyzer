/*----------------------------------------------------------------------
 * Analyzer.cs - Analyzer acts like an executive for code analysis
 * Ver 1.0
 * Language - C#, 2013, .Net Framework 4.5
 * Platform - Sony Vaio T14, Win 8.1
 * Application - CodeAnalyzer| Project #2| Fall 2014|
 * Author - Alok Arya (alarya@syr.edu)
 * ---------------------------------------------------------------------
 * Package operations:
 * Analyzer interacts with the File manger module to get file references.
 * Analyzer calls the Semi Expressions package for converting source code
 * files into semi expressions.
 * This package calls the parser via builder for code analysis.
 * The package also calls Display packge for displaying analysis results
 * and saving output to XML files.
 * 
 * Required files:
 * Semi.cs parser.cs toker.cs Display.cs
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysis
{
    public class Analyzer
    {
        
        //-------------------Calls Filemanager to get all files which need to be analyzed------------------------------------------
        public static List<String> getFiles(string path, List<String> patterns, bool recurse)
        {
            FileMgr fm = new FileMgr();
            foreach (string pattern in patterns)
                fm.addPattern(pattern);
                
            fm.findFiles(path,recurse);
            return fm.returnFiles();
        }

        //----------------------Prase 1 for type and function analysis------------------------------------------------------------------
        public static void parse1(List<String> files,bool relationship, bool xml)
        {
            List<Elem> typefunc = new List<Elem>();

            if (!relationship)
            {
                Console.Write("\n  Type and Function Analysis");
                Console.Write("\n ----------------------------\n");
            }            
            foreach (object file in files)
            {
                
                CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                semi.displayNewLines = false;
                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", file);
                    return;
                }

                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.build();
                Repository rep = Repository.getInstance();
                List<Elem> temp = rep.typefunc;
                Elem E = new Elem();
                E.type = "file";
                E.name = file.ToString();
                temp.Add(E);       //add file name 
                try
                {
                    while (semi.getSemi())
                        parser.parse(semi);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }

                
                typefunc.AddRange(rep.typefunc); //will be passed to Xml output
                semi.close();
            }

            if (!relationship)
            {
                Display d1 = new Display();
                d1.displaytypefunc(typefunc);     //call display package
                Console.ReadLine();
            }
            
            if(xml && !relationship)
            {
                Display d1 = new Display();
                d1.diplayXML1(typefunc);
            }
            Console.WriteLine("\n\n All the user types found :-");
            List<Elem> types = new List<Elem>();
            types = Repository.gettypes();
            foreach (Elem type in types)
            {
                Console.WriteLine("\n {0} {1}", type.type, type.name);
            }
            Console.ReadLine();
        }


        //----------------------Second parse... to find type relationships-------------------------------------------------------
        public static void parse2(List<String> files, bool xml)
        {
            Console.Write("\n  Type Relationships ");
            Console.Write("\n ----------------------------\n");
            Dictionary<String, List<Relationships>> reln = new Dictionary<string,List<Relationships>>();
            foreach (object file in files)
            {
                CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                semi.displayNewLines = false;

                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", file);
                    return;
                }
                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.build1();
                try
                {
                    while (semi.getSemi())
                        parser.parse(semi);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\nException encountered:{0}\n", ex.Message);
                }
                semi.close();
                //ordering relationship list by type1 and the relationship
                List<Relationships> temp = FinalRelationships.getrelationships().OrderBy(x => x.type1).ThenBy(x => x.relationship).ToList<Relationships>();
                reln.Add(file as string, temp);   // adding relationship analysis file wise in a dictionary
            }

            Display d2 = new Display();
            d2.displayrelationships(reln);
            if (xml)
                d2.displayXML2(reln);
        }
        
        
        //-------------------------------------------calls parse1 and parse2--------------------------------------------------------------
        public static void doAnalysis(string path, List<String> patterns, bool relationship, bool subdir, bool xml)
        {
            
            List<string> files = new List<string>();
            files = Analyzer.getFiles(path,patterns,subdir);
            if (files.Count == 0)
            {
                Console.WriteLine("\nNo files found....Please recheck the patterns provided");
                Console.ReadLine();
                return;
            }
            
            parse1(files,relationship,xml);

            if(relationship)
            parse2(files,xml);
        }

//<-------------test stub----------------------------------------------------------------------------->
#if(TEST_ANALYZER)     
        static void Main(string[] args)
        {
            string path = "../../";
            List<string> patterns = new List<string>();
            patterns.Add("*.cs");
            List<String> files = Analyzer.getFiles(path, patterns,false);
            doAnalysis(path, files,false,false,false);                   
        }
#endif    
    }
}