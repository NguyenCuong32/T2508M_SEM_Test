import { HttpRequest, HttpResponseInit } from "@azure/functions";
import { PlayerService } from "../services/player.service";
import { RegisterPlayerDto } from "../models/player.model";
import { successResponse, errorResponse } from "../shared/response.helper";

/**
 * Controller for player-related HTTP actions.
 * Parses the request, delegates to PlayerService, and formats the response.
 */
export class PlayerController {
  private readonly playerService: PlayerService;

  constructor() {
    this.playerService = new PlayerService();
  }

  /**
   * POST /api/registerplayer
   * Registers a new player.
   */
  async registerPlayer(req: HttpRequest): Promise<HttpResponseInit> {
    try {
      const body = (await req.json()) as RegisterPlayerDto;
      const player = await this.playerService.registerPlayer(body);

      return {
        status: 201,
        jsonBody: successResponse(player, "Player registered successfully."),
      };
    } catch (err) {
      const message = err instanceof Error ? err.message : "Unexpected error.";
      return {
        status: 400,
        jsonBody: errorResponse(message),
      };
    }
  }
}
