import { HttpClient, HttpParams } from "@angular/common/http";
import { map } from "rxjs";
import { PaginatedResult } from "../_models/pagination";


export function getPaginatedHeaders(pageNumber: number, pageSize: number) {
  let httpParams = new HttpParams();
  return httpParams.append('pageNumber', pageNumber.toString())
               .append('pageSize', pageSize.toString())
}

export function getPaginatedResult<T>(url: string, httpParams: HttpParams, http: HttpClient) {
  let paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();

  return http.get<T>(url, {observe: 'response', params: httpParams}).pipe(
    map(response => {
      paginatedResult.result = response.body;
      if (response.headers.get('Pagination') != null) {
        paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
      }
      return paginatedResult;
    })
  )
}
