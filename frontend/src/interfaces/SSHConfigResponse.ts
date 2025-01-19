import type { SSHConfig } from "../interfaces/SSHConfig.ts";

export interface SSHConfigResponse {
    $id: string;
    $values: SSHConfig[];
  }
  