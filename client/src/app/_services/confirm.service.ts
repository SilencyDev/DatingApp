import { Injectable } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Observable } from 'rxjs';
import { ConfirmationDialogComponent } from '../modal/confirmation-dialog/confirmation-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {
  bsModalRef: BsModalRef;

  constructor(private modalService: BsModalService) { }

  confirm(
    title = "Confirmation",
    message= 'Do you want to continue ?',
    btnOkText = 'Ok',
    btnCancelText = 'Cancel'
  ) : Observable<boolean> {
    const config = {
      initialState: {
        title,
        message,
        btnOkText,
        btnCancelText
      }
    }
    this.bsModalRef = this.modalService.show(ConfirmationDialogComponent, config);
    return new Observable<boolean>(this.getResult());
  }

  private getResult() {
    return (observer : any) => {
      const subscription = this.bsModalRef.onHidden.subscribe({
        next: () =>
          observer.next(this.bsModalRef.content.result),
      });
      return {
        unsubscribe() {
          subscription.unsubscribe();
        }
      }
    }
  }
}
