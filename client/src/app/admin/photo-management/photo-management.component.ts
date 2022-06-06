import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@microsoft/signalr';
import { Pagination } from 'src/app/_models/pagination';
import { Photo } from 'src/app/_models/photo';
import { AdminService } from 'src/app/_services/admin.service';
import { MembersService } from 'src/app/_services/members.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.scss']
})
export class PhotoManagementComponent implements OnInit {
  photoList: Photo[] = [];
  pagination: Pagination;
  constructor(private adminService: AdminService) { }

  ngOnInit(): void {
    this.loadWaitingForApproval();
  }

  loadWaitingForApproval() {
    this.adminService.getPhotosToModerate(this.adminService.userParams).subscribe({
      next: result => {
        this.photoList = result.result;
        this.pagination = result.pagination;
      }
    })
  }

  validatePhoto(photo: Photo) {
    this.adminService.validatePhoto(photo).subscribe({
      next: () => {
        let result = this.photoList.findIndex(p => p.id == photo.id);
        if (result != null)
          this.photoList.splice(result, 1);
      }
    })
  }

  removePhoto(photo: Photo) {
    this.adminService.removePhoto(photo).subscribe({
      next: () => {
        let result = this.photoList.findIndex(p => p.id == photo.id);
        if (result != null)
          this.photoList.splice(result, 1);
      }
    })
  }

  onPageChange(event: any) {
    if (this.adminService.userParams.pageNumber == event.page)
      return;
    this.adminService.userParams.pageNumber = event.page;
    this.loadWaitingForApproval();
  }
}
