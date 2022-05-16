import { Component, OnInit } from '@angular/core';
import { Observable, take } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { User } from 'src/app/_models/user';
import { UserParams } from 'src/app/_models/userParams';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.scss']
})
export class MemberListComponent implements OnInit {
  members: Member[];
  pagination: Pagination;
  genderList = [
    {value: 'male', display: 'males'},
    {value: 'female', display: 'females'},
    {value: 'non-binary', display: 'non-binary'}]

  constructor(public memberService: MembersService) {
  }

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers() {
      this.memberService.getMembers(this.memberService.userParams).subscribe({
      next: response => {
        this.members = response.result;
        this.pagination = response.pagination;
      }
    });
  }

  resetFilter() {
    this.memberService.resetUserParams();
    this.loadMembers();
  }

  onPageChange(event: any) {
    this.memberService.userParams.pageNumber = event.page;
    this.loadMembers();
  }

}
