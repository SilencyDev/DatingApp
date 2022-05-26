import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router, RouteReuseStrategy } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.scss']
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  @ViewChild('memberTabs', {static:true}) memberTabs: TabsetComponent;
  activeTab: TabDirective;
  member: Member;
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];
  messages: Message[] = [];
  user: User;

  constructor(
    public memberService: MembersService,
    private accountService: AccountService,
    public presence: PresenceService,
    private route: ActivatedRoute,
    private toastr: ToastrService,
    private messageService: MessageService,
    private router: Router) {
		this.accountService.currentUser$.pipe(take(1)).subscribe({
			next: user => this.user = user
		});
		this.router.routeReuseStrategy.shouldReuseRoute = () => false;
	}

  ngOnInit(): void {
    this.route.data.subscribe({
      next: data => {
        this.member = data.member}
    })

    this.route.queryParams.subscribe(params => {
      params.tab ? this.selectTab(params.tab) : this.selectTab(0)
    })

    this.galleryOptions = [{
      width: '500px',
      height: '500px',
      imagePercent: 100,
      thumbnailsColumns: 4,
      imageAnimation: NgxGalleryAnimation.Slide,
      preview: false
    }];
    this.galleryImages = this.getImages();
  }

  like(member: Member) {
    this.memberService.like(member.username).subscribe({
      next: response => this.toastr.success('You liked ' + member.username)
    });
  }

  getImages() {
    let imageUrls = [];
    for (const photo of this.member.photos) {
      imageUrls.push({
        small: photo.url,
        medium: photo.url,
        big: photo.url,
      })
    }
    return imageUrls;
  }

  loadMember(): void {
    this.memberService.getMember(this.route.snapshot.paramMap.get('username')).subscribe({
      next: result => this.member = result
    })
  }

  loadMessages() {
    this.messageService.getMessagesThread(this.member.username).subscribe({
      next: response => {
        this.messages = response;
      }
    })
  }

  selectTab(tabId: number) {
    this.memberTabs.tabs[tabId].active = true;
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading === 'Messages' && this.messages.length === 0) {
      this.messageService.CreateConnectionHub(this.user, this.member.username);
    } else {
      this.messageService.DestroyHubConnection();
    }
  }

  ngOnDestroy(): void {
    this.messageService.DestroyHubConnection();
  }

}
