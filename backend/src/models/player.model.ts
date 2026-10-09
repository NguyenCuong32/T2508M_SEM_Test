/**
 * Represents a row in the [Player] table.
 */
export interface Player {
  PlayerId: string;
  PlayerName: string;
  FullName: string;
  Age: string;
  Level: number;
  Email: string;
}

/**
 * Payload expected when registering a new player (PlayerId is auto-generated).
 */
export interface RegisterPlayerDto {
  PlayerName: string;
  FullName: string;
  Age: string;
  Level?: number;
  Email: string;
}
