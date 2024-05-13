using Microsoft.IdentityModel.Tokens;

public static class ParticipantsRoute {
  public static void GetRoutes(WebApplication app, WebApplicationBuilder builder) {
    UsersService usersService = new UsersService(builder);
    AuthService authService = new AuthService(builder);
    ParticipantsService participantsService = new ParticipantsService(builder);

    app.MapGet("/participants", async (HttpRequest request) => {
      var participants = await participantsService.GetAllParticipants();

      return Results.Ok(participants);
    });

    app.MapPost("/participants/{tournament}", async (HttpContext context, string tournament) => {
      IFormFile file = context.Request.Form.Files[0];

      List<Participant> participants = new List<Participant>();

      using (StreamReader reader = new StreamReader(file.OpenReadStream())) {
        string rawCsv = reader.ReadToEnd();

        List<string> csvRows = rawCsv.Split('\n').ToList();

        csvRows.RemoveAt(0);

        foreach (string row in csvRows) {
          if (row.IsNullOrEmpty()) continue;

          List<string> fields = row.Split(',').ToList();

          Participant participant = new Participant(
            Guid.NewGuid().ToString(),
            tournament,
            fields[2],
            fields[4],
            fields[0],
            false
          );

          participants.Add(participant);
        }
      }

      List<Participant> registeredParticipants = await participantsService.GetAllParticipants();

      List<Participant> newParticipants = participants.Where(newParticipant => 
        !registeredParticipants.Exists(registeredParticipant => 
          newParticipant.tournament.Equals(registeredParticipant.tournament) &&
          newParticipant.name.Equals(registeredParticipant.name) &&
          newParticipant.society.Equals(registeredParticipant.society) &&
          newParticipant.subscribedAt.Equals(registeredParticipant.subscribedAt)
        )
      ).ToList();

      newParticipants.ForEach(async participant => 
        await participantsService.CreateParticipant(participant)
      );

      return await participantsService.GetAllParticipants();
    });

    app.MapPost("/participant/{tournament}/pfp", async (HttpContext context, string tournament) => {
      IFormFile file = context.Request.Form.Files[0];

      using (MemoryStream stream = new MemoryStream()) {
        await file.CopyToAsync(stream);

        await participantsService.UploadParticipantPfp(tournament, file.FileName, stream);
      }

      return Results.Ok();
    });
  }
}