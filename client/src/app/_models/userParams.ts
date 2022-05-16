import { User } from "./user";

export class UserParams {
  gender = "female";
  minAge = 18;
  maxAge = 150;
  pageNumber = 1;
  pageSize = 5;
  orderBy = "lastActive";
  constructor(user: User) {
    // futur filter
  }
}
