import axios from "axios";
import { ApiResponse, PlayerAssetReport } from "../types/player-asset.types";

const BASE_URL = "/api";

/**
 * Fetches the player-assets report from the Azure Function.
 * Endpoint: GET /api/getassetsbyplayer
 */
export async function fetchPlayerAssetsReport(): Promise<PlayerAssetReport[]> {
  const { data } = await axios.get<ApiResponse<PlayerAssetReport[]>>(
    `${BASE_URL}/getassetsbyplayer`
  );

  if (!data.success || !data.data) {
    throw new Error(data.message ?? "Failed to fetch report.");
  }

  return data.data;
}
