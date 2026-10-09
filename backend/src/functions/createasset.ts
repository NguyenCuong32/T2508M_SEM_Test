import { app, HttpRequest, HttpResponseInit, InvocationContext } from "@azure/functions";
import { AssetController } from "../controllers/asset.controller";

const assetController = new AssetController();

/**
 * Azure Function: createasset
 * Route : POST /api/createasset
 * Body  : { AssetName, LevelRequire }
 */
async function createAsset(
  req: HttpRequest,
  context: InvocationContext
): Promise<HttpResponseInit> {
  context.log("createasset triggered");
  return assetController.createAsset(req);
}

app.http("createasset", {
  methods: ["POST"],
  authLevel: "anonymous",
  route: "createasset",
  handler: createAsset,
});
