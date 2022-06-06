import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { take } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Photo } from '../_models/photo';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { getPaginatedHeaders, getPaginatedResult } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  apiUrl = environment.apiUrl;
  userParams: UserParams;
  user: User;

  constructor(private http: HttpClient, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        this.user = user;
        this.userParams = new UserParams(this.user);
      }
    });
  }

  getUsersWithRoles() {
    return this.http.get<Partial<User[]>>(this.apiUrl + 'admin/users-with-roles');
  }

  editUserRoles(username: string, roles: string[]) {
    return this.http.post(this.apiUrl + 'admin/edit-roles/' + username + '?roles=' + roles, {});
  }

  getPhotosToModerate(userParams: UserParams) {
    let httpParams = getPaginatedHeaders(userParams.pageNumber, userParams.pageSize);
    return getPaginatedResult<Photo[]>(this.apiUrl + 'admin/photos-to-moderate', httpParams, this.http);
  }

  validatePhoto(photo: Photo) {
    return this.http.post(this.apiUrl + 'admin/photos-to-moderate/accept/' + photo.id.toString(), {});
  }

  removePhoto(photo: Photo) {
    return this.http.delete(this.apiUrl + 'admin/photos-to-moderate/delete/' + photo.id.toString(), {});
  }
}
