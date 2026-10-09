import { app, HttpRequest, HttpResponseInit, InvocationContext } from "@azure/functions";
import { PlayerAssetController } from "../controllers/player-asset.controller";

const playerAssetController = new PlayerAssetController();

/**
 * Azure Function: getassetsbyplayer
 * Route : GET /api/getassetsbyplayer
 * Returns the report: No | PlayerName | Level | Age | AssetName
 */
async function getAssetsByPlayer(
  req: HttpRequest,
  context: InvocationContext
): Promise<HttpResponseInit> {
  context.log("getassetsbyplayer triggered");
  return playerAssetController.getAssetsByPlayer();
}

app.http("getassetsbyplayer", {
  methods: ["GET"],
  authLevel: "anonymous",
  route: "getassetsbyplayer",
  handler: getAssetsByPlayer,
});
