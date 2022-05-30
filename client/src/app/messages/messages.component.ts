import { registerLocaleData } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { Pagination } from '../_models/pagination';
import { ConfirmService } from '../_services/confirm.service';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss']
})
export class MessagesComponent implements OnInit {
  messages: Message[] = [];
  pagination: Pagination;
  container = "Unread";
  pageNumber = 1;
  pageSize = 5;
  loading = false;

  constructor(private messageService: MessageService, private confirmService: ConfirmService) { }

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    this.loading = true;
    this.messageService.getMessages(this.pageNumber, this.pageSize, this.container).subscribe({
      next: response => {
        this.messages = response.result;
        this.pagination = response.pagination;
        this.loading = false;
      }
    })
  }

  deleteMessage(message: Message) {
    this.confirmService.confirm('Do you really want to delete the message ?', 'This cannot be undone').subscribe({
      next: result => {
        if (result) {
          this.messageService.deleteMessage(message.id).subscribe({
            next: response => {
              let index = this.messages.findIndex(m => m.id == message.id);
            this.messages.splice(index, 1);}
          });
        }
      }
    })
  }

  onPageChanged(event: any) {
    if (this.pageNumber == event.page)
      return;
    this.pageNumber = event.page;
    this.loadMessages();
  }
}
