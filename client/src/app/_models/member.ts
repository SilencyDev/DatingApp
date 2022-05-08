import { Photo } from "./photo";

export interface Member {
  id: number;
  username: string;
  photoUrl: string;
  pseudo: string;
  gender: string;
  introduction: string;
  lookingFor: string;
  interests: string;
  city: string;
  country: string;
  age: number;
  created: Date;
  lastActive: Date;
  photos: Photo[];
}
