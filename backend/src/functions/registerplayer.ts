import { app, HttpRequest, HttpResponseInit, InvocationContext } from "@azure/functions";
import { PlayerController } from "../controllers/player.controller";

const playerController = new PlayerController();

/**
 * Azure Function: registerplayer
 * Route : POST /api/registerplayer
 * Body  : { PlayerName, FullName, Age, Level?, Email }
 */
async function registerPlayer(
  req: HttpRequest,
  context: InvocationContext
): Promise<HttpResponseInit> {
  context.log("registerplayer triggered");
  return playerController.registerPlayer(req);
}

app.http("registerplayer", {
  methods: ["POST"],
  authLevel: "anonymous",
  route: "registerplayer",
  handler: registerPlayer,
});
