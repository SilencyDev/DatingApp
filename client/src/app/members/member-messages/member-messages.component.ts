import { ChangeDetectionStrategy, Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.scss']
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild('messageForm') messageForm: NgForm
  @Input() messages: Message[] = [];
  @Input() username: string;
  content: string;

  constructor(public messageService: MessageService) { }

  ngOnInit(): void {
  }

  sendMessage() {
    if (this.content == '')
      return;
    this.messageService.sendMessage(this.username, this.content).then(
      () => this.messageForm.reset());
  }
}
