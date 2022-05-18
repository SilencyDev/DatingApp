import { PaginationParams } from "./paginationParams";

export class LikeParams extends PaginationParams {
  predicate: string;
  constructor(predicate: string) {
    super()
    this.predicate = predicate;
  }
}
