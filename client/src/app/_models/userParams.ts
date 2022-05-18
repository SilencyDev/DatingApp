import { PaginationParams } from "./paginationParams";
import { User } from "./user";

export class UserParams extends PaginationParams {
  gender = "female";
  minAge = 18;
  maxAge = 150;
  orderBy = "lastActive";
  constructor(user: User) {
    super()
    // futur filter
  }
}
