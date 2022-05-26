export interface Group {
  name: string;
  connections: Connection[]
}

export interface Connection {
  username: string;
  connectionId: string;
}
