import { PlayerRepository } from "../repositories/player.repository";
import { Player, RegisterPlayerDto } from "../models/player.model";

/**
 * Business-logic layer for Player operations.
 */
export class PlayerService {
  private readonly playerRepo: PlayerRepository;

  constructor() {
    this.playerRepo = new PlayerRepository();
  }

  /**
   * Registers a new player after validating required fields.
   * @throws Error when any required field is missing or invalid.
   */
  async registerPlayer(dto: RegisterPlayerDto): Promise<Player> {
    if (!dto.PlayerName || dto.PlayerName.trim() === "") {
      throw new Error("PlayerName is required.");
    }
    if (!dto.FullName || dto.FullName.trim() === "") {
      throw new Error("FullName is required.");
    }
    if (!dto.Age || dto.Age.trim() === "") {
      throw new Error("Age is required.");
    }
    if (!dto.Email || dto.Email.trim() === "") {
      throw new Error("Email is required.");
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(dto.Email)) {
      throw new Error("Email format is invalid.");
    }

    return this.playerRepo.create({
      PlayerName: dto.PlayerName.trim(),
      FullName: dto.FullName.trim(),
      Age: dto.Age.trim(),
      Level: dto.Level ?? 1,
      Email: dto.Email.trim(),
    });
  }

  async getAllPlayers(): Promise<Player[]> {
    return this.playerRepo.findAll();
  }
}
