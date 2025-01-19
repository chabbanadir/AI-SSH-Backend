export interface SSHConfig {
    id: string;
    hostname: string;
    port: number;
    username: string;
    authType: string;
    passwordOrKeyPath: string;
    userId: string;
    sshDefaultConfig: boolean;
  }