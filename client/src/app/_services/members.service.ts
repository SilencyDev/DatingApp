import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable, of, take } from 'rxjs';
import { environment } from 'src/environments/environment';
import { LikeParams } from '../_models/likeParams';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  members: Member[] = [];
  apiUrl = environment.apiUrl;
  userParams: UserParams;
  user: User;

  constructor(private http: HttpClient, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.userParams = new UserParams(user);
    })
  }

  getMembers(userParams: UserParams) {
    let httpParams = this.getPaginatedHeaders(userParams.pageNumber, userParams.pageSize);
    httpParams = httpParams.append('gender', userParams.gender)
              .append('orderBy', userParams.orderBy)
              .append('maxAge', userParams.maxAge.toString())
              .append('minAge', userParams.minAge.toString());

    return this.getPaginatedResult<Member[]>(this.apiUrl + 'users', httpParams);
  }

  getUserParams() {
    return this.userParams;
  }

  setUserParams(params: UserParams) {
    this.userParams = params;
  }

  resetUserParams() {
    this.userParams = new UserParams(this.user);
  }

  like(username: string) {
    return this.http.post(this.apiUrl + 'likes/' + username, {});
  }

  getLikes(likeParams: LikeParams) {
    let httpParams = this.getPaginatedHeaders(likeParams.pageNumber, likeParams.pageSize);
    httpParams = httpParams.append('predicate', likeParams.predicate);
    return this.getPaginatedResult<Partial<Member[]>>(this.apiUrl + 'likes?predicate=' + likeParams.predicate, httpParams);
  }

  getMember(username: string) {
    let find = this.members.find(user => user.username === username);
    if (find != undefined) return of(find);
    return this.http.get<Member>(this.apiUrl + 'users/' + username)
  }

  updateMember(member: Member) {
    return this.http.put(this.apiUrl + 'users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        if (index >= 0)
          this.members[index] = member;
      })
    )
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.apiUrl + 'users/delete-photo/' + photoId);
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.apiUrl + 'users/set-main-photo/' + photoId, {});
  }

  private getPaginatedHeaders(pageNumber: number, pageSize: number) {
    let httpParams = new HttpParams();
    return httpParams.append('pageNumber', pageNumber.toString())
                 .append('pageSize', pageSize.toString())
  }

  private getPaginatedResult<T>(url: string, httpParams: HttpParams) {
    let paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();

    return this.http.get<T>(url, {observe: 'response', params: httpParams}).pipe(
      map(response => {
        paginatedResult.result = response.body;
        if (response.headers.get('Pagination') != null) {
          paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
        }
        return paginatedResult;
      })
    )
  }
}
