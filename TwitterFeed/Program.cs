using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Configuration;


namespace TwitterFeed
{
    class Program
    {
        //List for total users
        static List<string> usersList = new List<string>();

        //List of followers and who they follow held in memory
        static List<Follower> followerList = new List<Follower>();
        //Trimmed follower list for output display purposes
        static List<Follower> outputFollerList = new List<Follower>();

        //List of tweets and who they are performed by
        static List<Tweet> tweetList = new List<Tweet>();
       
        static void Main(string[] args)
        {            
            //Read, validate and populate inputs into memory
            ReadUserInput(ConfigurationManager.AppSettings["users"]);
            PopulateTweetList(ConfigurationManager.AppSettings["tweets"]);

            //Remove Duplicates from User list
            usersList = RemoveDuplicateUsers();

            //Populate Output Follower List containing distinct Followers
            PopulateOutputFollowerList();

            //Sort objects in memory and write to console
            WriteConsoleOutput();

            //Display console output
            Console.ReadLine();
        }

        private static void ReadUserInput(string inputFile)
        {
            using (StreamReader sr = new StreamReader(inputFile))
            {
                string line;
                int lineCount = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    lineCount++;
                    ValidateInput(line, inputFile, lineCount);
                    PopulateUserLists(line);
                }
            }
        }

        private static void PopulateTweetList(string inputFile)
        {
            using (StreamReader sr = new StreamReader(inputFile))
            {
                string line;
                int lineCount = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    lineCount++;
                    ValidateInput(line, inputFile, lineCount);
                    Tweet tweet = new Tweet();
                    tweet.twitter = line.Split('>')[0];
                    tweet.tweet = line.Split('>')[1];

                    tweetList.Add(tweet);                   
                }
            }
        }

        private static void PopulateUserLists(string inputValue)
        {
            //split input list
            List<string> tempUserList = new List<string>();
            tempUserList = inputValue.Split(new Char[] { ',', ' '}).ToList();
            tempUserList.Remove("follows");
            tempUserList.Remove(string.Empty);

            Follower follower = new Follower();
            follower.name = tempUserList[0];
            follower.follows = new List<string>();

            //Determine if follower already in list
            List<Follower> pastFollower = new List<Follower>();

            //populate List of followers
            for (int i=0; i < tempUserList.Count; i++)
            {
                if (i == 0)
                {
                    //At position 0 are where the followers exist hence only over user list are populated                  
                    usersList.Add(tempUserList[i]);
                }
                else
                {
                    //position great then zero are people being followed so both lists to be populate            
                    follower.follows.Add(tempUserList[i]);
                    usersList.Add(tempUserList[i]);
                }               
            }

            followerList.Add(follower);
            //PopulateFollowerList(follower);
        }

        private static List<string> RemoveDuplicateUsers()
        {
            return usersList.Distinct().ToList();
        }

        private static void WriteConsoleOutput()
        {
            //Sort overall user list alphabetically
            usersList.Sort();

            //Write output to console
            foreach(string name in usersList)
            {
                Console.WriteLine(name);
                PopulateUserFeed(name);
            }           
        }

        private static void PopulateUserFeed(string twitterUser)
        {
            //Build and display user twitter feed  
            var outputFollows = outputFollerList.Where(x => x.name == twitterUser).Select(x => x.follows).ToList();
            
            if(outputFollows.Count() == 0)
            {
                //Conditions that is met when user does not follow anyone
                foreach (Tweet tw in tweetList)
                {
                    if (tw.twitter == twitterUser)
                    {
                        Console.WriteLine("@{0}: {1}", tw.twitter, tw.tweet);
                    }
                }
            }
            else
            {
                //Condition that is met if user has followers
                foreach (var item in outputFollows)
                {
                    foreach (Tweet tw in tweetList)
                    {
                        if (tw.twitter == twitterUser || item.Contains(tw.twitter))
                        {
                            Console.WriteLine("@{0}: {1}", tw.twitter, tw.tweet);
                        }
                    }
                }

            }
        }

        private static void PopulateOutputFollowerList()
        {
            foreach (Follower fllwr in followerList)
            {
                if (outputFollerList.Count == 0 || outputFollerList.Select(x => x.name != fllwr.name).First())
                {
                    var tempFollowerList = followerList
                                            .Select(i => new { i.name, i.follows })
                                            .Where(x => x.name == fllwr.name)
                                            .ToList();

                    //Add to output List
                    List<string> followsList = new List<string>();
                    foreach (var item in tempFollowerList)
                    {
                        var follows = item.follows.ToList();
                        //Foreach person that is followed
                        foreach (var person in follows)
                        {
                            followsList.Add(person);
                        }
                    }

                    //Populate final output follower list
                    Follower outputFollower = new Follower();
                    outputFollower.name = tempFollowerList[0].name;
                    outputFollower.follows = followsList.Distinct().ToList();

                    outputFollerList.Add(outputFollower);
                }
            }
        }

        private static void ValidateInput(string input, string file, int lineCount)
        {
            int errorCount = 0;
            if (input == string.Empty)
            {
                errorCount++;
                Console.WriteLine(String.Format("File contains empty string on line {0} - File: {1}", lineCount, file));
            }

            if (input.Contains(">"))
            {
                List<string> messages = input.Split('>').ToList();
                if(messages[1].Length > 140)
                {
                    errorCount++;
                    Console.WriteLine(String.Format("File contains tweet then 140 characters on line {0} - File: {1}", lineCount, file));
                }
            }

            if (errorCount > 0)
                Console.ReadLine();
        }
    }
}
