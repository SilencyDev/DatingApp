import { Component, OnInit } from '@angular/core';
import { LikeParams } from '../_models/likeParams';
import { Member } from '../_models/member';
import { Pagination } from '../_models/pagination';
import { MembersService } from '../_services/members.service';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.scss']
})
export class ListsComponent implements OnInit {
  members: Partial<Member[]>;

  likeParams: LikeParams;
  pagination: Pagination;

  constructor(private memberService: MembersService) {
    this.likeParams = new LikeParams('likedby');
    this.likeParams.pageNumber = 1;
    this.likeParams.pageSize = 5;
  }

  ngOnInit(): void {
    this.loadMembers();
  }

  onPageChange(event: any) {
    this.likeParams.pageNumber = event.page;
    this.loadMembers();
  }

  loadMembers() {
    this.memberService.getLikes(this.likeParams).subscribe({
      next: response => {
        this.members = response.result;
        this.pagination = response.pagination;
      }
    })
  }

}
