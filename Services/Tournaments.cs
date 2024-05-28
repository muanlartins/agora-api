using System.Xml;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

public class TournamentsService {
  public AmazonS3Client s3;
  public TournamentsService(WebApplicationBuilder builder) {
    BasicAWSCredentials credentials = new BasicAWSCredentials(
      builder.Configuration["AWS:AccessKey"], 
      builder.Configuration["AWS:SecretKey"]
    );
    s3 = new AmazonS3Client(credentials, RegionEndpoint.SAEast1);
  }

  public async Task<string> GetTournamentTabbyFileContent(string tournament) {
    string fileName = tournament + ".xml";

    GetObjectRequest request = new GetObjectRequest {
      BucketName = "tabbyarchive",
      Key = fileName,
    };

    using (GetObjectResponse response = await s3.GetObjectAsync(request))
    using (Stream responseStream = response.ResponseStream)
    using (StreamReader reader = new StreamReader(responseStream)) {
      string content = reader.ReadToEnd();
      return content;
    }
  }

  public async Task<object> GetTournamentTabbyData(string tournament) {
    string xmlContent = await GetTournamentTabbyFileContent(tournament);

    XmlDocument doc = new XmlDocument();
    doc.LoadXml(xmlContent);
  
    List<object> speakerCategories = new List<object>();
    Dictionary<string, string> speakerCategory = new Dictionary<string, string>();

    XmlNodeList speakerCategoryNodes = doc.SelectNodes("//speaker-category")!;
    foreach (XmlNode speakerCategoryNode in speakerCategoryNodes) {
      speakerCategories.Add(new {
        id = speakerCategoryNode.Attributes!["id"]?.Value,
        speakerCategory = speakerCategoryNode.InnerText,
      });

      if (speakerCategoryNode.Attributes["id"] is not null) 
        speakerCategory[speakerCategoryNode.Attributes["id"]!.Value] = speakerCategoryNode.InnerText;
    }

    List<object> breakCategories = new List<object>();
    Dictionary<string, string> breakCategory = new Dictionary<string, string>();

    XmlNodeList breakCategoryNodes = doc.SelectNodes("//break-category")!;
    foreach (XmlNode breakCategoryNode in breakCategoryNodes) {
      breakCategories.Add(new {
        id = breakCategoryNode.Attributes!["id"]?.Value,
        breakCategory = breakCategoryNode.InnerText,
      });

      if (breakCategoryNode.Attributes["id"] is not null) 
        breakCategory[breakCategoryNode.Attributes["id"]!.Value] = breakCategoryNode.InnerText;
    }

    List<object> institutions = new List<object>();
    Dictionary<string, string> institution = new Dictionary<string, string>();
    
    XmlNodeList institutionNodes = doc.SelectNodes("//institution")!;
    foreach (XmlNode institutionNode in institutionNodes) {
      institutions.Add(new {
        id = institutionNode.Attributes!["id"]?.Value,
        name = institutionNode.InnerText
      });

      if (institutionNode.Attributes["id"] is not null) 
        institution[institutionNode.Attributes["id"]!.Value] = institutionNode.InnerText;
    }

    List<object> adjudicators = new List<object>();
    Dictionary<string, object> participant = new Dictionary<string, object>();


    XmlNodeList adjudicatorNodes = doc.SelectNodes("//adjudicator")!;
    foreach (XmlNode adjudicatorNode in adjudicatorNodes) {
      adjudicators.Add(new {
        id = adjudicatorNode.Attributes!["id"]?.Value,
        name = adjudicatorNode.Attributes["name"]?.Value,
        society = adjudicatorNode.Attributes["institutions"] is not null ? institution[adjudicatorNode.Attributes["institutions"]!.Value] : null,
      });

      if (adjudicatorNode.Attributes["id"] is not null) 
        participant[adjudicatorNode.Attributes["id"]!.Value] = new {
          name = adjudicatorNode.Attributes["name"]?.Value,
          society = adjudicatorNode.Attributes["institutions"] is not null ? institution[adjudicatorNode.Attributes["institutions"]!.Value] : null,
        };
    }

    List<object> speakers = new List<object>();

    XmlNodeList speakerNodes = doc.SelectNodes("//speaker")!;
    foreach (XmlNode speakerNode in speakerNodes) {
      speakers.Add(new {
        id = speakerNode.Attributes!["id"]?.Value,
        name = speakerNode.InnerText,
        society = speakerNode.Attributes["institutions"] is not null ? institution[speakerNode.Attributes["institutions"]!.Value] : null,
        speakerCategories = 
          speakerNode.Attributes["categories"] is not null &&
          speakerNode.Attributes["categories"]!.Value != "" ? 
          speakerNode.Attributes!["categories"]!.Value.Split(" ").Select(category => speakerCategory[category]) : 
          null
      });

      if (
        speakerNode.Attributes["id"] is not null &&
        speakerNode.Attributes["categories"]!.Value != ""
      ) 
        participant[speakerNode.Attributes["id"]!.Value] = new {
          name = speakerNode.Attributes["name"]?.Value,
          society = speakerNode.Attributes["institutions"] is not null ? institution[speakerNode.Attributes["institutions"]!.Value] : null,
          speakerCategory = 
            speakerNode.Attributes["categories"] is not null &&
            speakerNode.Attributes["categories"]!.Value != "" ? 
            speakerNode.Attributes!["categories"]!.Value.Split(" ").Select(category => speakerCategory[category]) : 
            null
        };
    }

    List<object> participants = adjudicators.Concat(speakers).ToList();

    List<object> teams = new List<object>();
    Dictionary<string, object> team = new Dictionary<string, object>();

    XmlNodeList teamNodes = doc.SelectNodes("//team")!;
    foreach (XmlNode teamNode in teamNodes) {
      List<object> teamSpeakers = new List<object>();

      XmlNodeList teamSpeakerNodes = teamNode.SelectNodes(".//speaker")!;
      foreach (XmlNode speakerNode in teamSpeakerNodes)
      {
        teamSpeakers.Add(new {
          id = speakerNode.Attributes!["id"]?.Value,
          name = speakerNode.InnerText,
          society = speakerNode.Attributes["institutions"] is not null ? institution[speakerNode.Attributes["institutions"]!.Value] : null,
          speakerCategory = 
            speakerNode.Attributes["categories"] is not null &&
            speakerNode.Attributes["categories"]!.Value != "" ? 
            speakerNode.Attributes!["categories"]!.Value.Split(" ").Select(category => speakerCategory[category]) : 
            null
        });

        if (
          speakerNode.Attributes!["id"] is not null && 
          speakerNode.Attributes!["id"]!.Value != ""
        )
          participant[speakerNode.Attributes!["id"]!.Value] = new {
            name = speakerNode.InnerText,
            society = speakerNode.Attributes["institutions"] is not null ? institution[speakerNode.Attributes["institutions"]!.Value] : null,
            speakerCategory = 
              speakerNode.Attributes["categories"] is not null &&
              speakerNode.Attributes["categories"]!.Value != "" ? 
              speakerNode.Attributes!["categories"]!.Value.Split(" ").Select(category => speakerCategory[category]) : 
              null
          };
      }

      teams.Add(new {
        id = teamNode.Attributes!["id"]?.Value,
        name = teamNode.Attributes["name"]?.Value,
        speakers = teamSpeakers,
        breakCategory = 
          teamNode.Attributes["break-eligibilities"] is not null &&
          teamNode.Attributes["break-eligibilities"]!.Value != "" ? 
          teamNode.Attributes!["break-eligibilities"]!.Value.Split(" ").Select(category => breakCategory[category]) : 
          null,
      });

      if (teamNode.Attributes!["id"] is not null) 
        team[teamNode.Attributes!["id"]!.Value] = new {
          name = teamNode.Attributes["name"]?.Value,
          speakers = teamSpeakers,
          breakCategory = 
            teamNode.Attributes["break-eligibilities"] is not null &&
            teamNode.Attributes["break-eligibilities"]!.Value != "" ? 
            teamNode.Attributes!["break-eligibilities"]!.Value.Split(" ").Select(category => breakCategory[category]) :
            null
        };
    }

    List<object> motions = new List<object>();
    Dictionary<string, string> motion = new Dictionary<string, string>();

    XmlNodeList motionNodes = doc.SelectNodes("//motion")!;
    foreach (XmlNode motionNode in motionNodes) {
      motions.Add(new {
        id = motionNode.Attributes!["id"]?.Value,
        motion = motionNode.InnerText,
      });

      if (motionNode.Attributes["id"] is not null) 
        motion[motionNode.Attributes["id"]!.Value] = motionNode.InnerText;
    }

    List<object> venues = new List<object>();
    Dictionary<string, string> venue = new Dictionary<string, string>();

    XmlNodeList venueNodes = doc.SelectNodes("//venue")!;
    foreach (XmlNode venueNode in venueNodes) {
      venues.Add(new {
        id = venueNode.Attributes!["id"]?.Value,
        venue = venueNode.InnerText,
      });

      if (venueNode.Attributes["id"] is not null) 
        venue[venueNode.Attributes["id"]!.Value] = venueNode.InnerText;
    }

    string tournamentName = doc.SelectSingleNode("//tournament")!.Attributes!["name"]!.Value;

    List<object> rounds = new List<object>();

    XmlNodeList roundNodes = doc.SelectNodes("//round")!;
    foreach (XmlNode roundNode in roundNodes) {
      string roundName = roundNode.Attributes!["name"]!.Value;
      string roundAbbreviation = roundNode.Attributes!["abbreviation"]!.Value;
      bool isEliminationRound = bool.Parse(roundNode.Attributes!["elimination"]!.Value);

      XmlNodeList debateNodes = roundNode.SelectNodes(".//debate")!;

      List<object> debates = new List<object>();

      foreach (XmlNode debateNode in debateNodes) {
        string debateId = debateNode.Attributes!["id"]!.Value;
        string[] debateAdjudicatorIds = debateNode.Attributes!["adjudicators"]!.Value.Split(" ");
        string debateChairId = debateNode.Attributes!["chair"]!.Value;
        string debateVenueId = debateNode.Attributes!["venue"]!.Value;
        string? debateMotionId = debateNode.Attributes!["motion"] is not null ? debateNode.Attributes!["motion"]!.Value : null;

        List<object> debateAdjudicators = debateAdjudicatorIds.Select(adjudicatorId => participant[adjudicatorId]).ToList();
        object debateChair = participant[debateChairId];
        string debateVenue = venue[debateVenueId];
        string? debateMotion = debateMotionId is not null ? motion[debateMotionId] : null;

        List<object> debateSides = new List<object>();

        XmlNodeList sideNodes = debateNode.SelectNodes(".//side")!;

        foreach (XmlNode sideNode in sideNodes) {
          object sideTeam = team[sideNode.Attributes!["team"]!.Value];

          XmlNode ballotNode = sideNode.SelectSingleNode(".//ballot")!;

          string sideRank = ballotNode.Attributes!["rank"]!.Value;
          string sideSps = ballotNode.InnerText;

          XmlNodeList speechNodes = sideNode.SelectNodes(".//speech")!;

          List<object> sideSpeakers = new List<object>();

          foreach (XmlNode speechNode in speechNodes) {
            object speechSpeaker = participant[speechNode.Attributes!["speaker"]!.Value];

            XmlNode speechBallotNode = speechNode.SelectSingleNode(".//ballot")!;
            string speechSps = speechBallotNode.InnerText;

            sideSpeakers.Add(new {
              speechSpeaker,
              speechSps
            });
          }

          debateSides.Add(new {
            sideTeam,
            sideSpeakers,
            sideSps,
            sideRank,
          });
        }

        debates.Add(new {
          debateId,
          debateAdjudicators,
          debateChair,
          debateVenue,
          debateMotion,
          debateSides
        });  
      }

      rounds.Add(new {
        roundName,
        roundAbbreviation,
        isEliminationRound,
        debates
      });
    }

    return new {
      institutions,
      adjudicators,
      speakers,
      participants,
      teams,
      motions,
      venues,
      speakerCategories,
      breakCategories,
      tournamentName,
      rounds
    };
  }
}