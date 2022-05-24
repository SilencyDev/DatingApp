import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from 'src/app/modal/roles-modal/roles-modal.component';
import { User } from 'src/app/_models/user';
import { AdminService } from 'src/app/_services/admin.service';
import { __values } from 'tslib';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.scss']
})
export class UserManagementComponent implements OnInit {
  users: Partial<User[]>;
  bsModalRef: BsModalRef;

  constructor(private adminService: AdminService, private modalService: BsModalService) { }

  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  getUsersWithRoles() {
    this.adminService.getUsersWithRoles().subscribe({
      next: result => {
        this.users = result;
      }
    })
  }

  OpenModal(user: User) {
    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        user,
        roles: this.getRolesArray(user)
      }
    };
    this.bsModalRef = this.modalService.show(RolesModalComponent, config);
    this.bsModalRef.content.updateSelectedRoles.subscribe({
      next: result => {
        const rolestoUpdate = {
          roles: [...result.filter(
              element => element.checked === true).map(
                  element => element.name)]
        };
        if (rolestoUpdate) {
          this.adminService.editUserRoles(user.username, rolestoUpdate.roles).subscribe({
            next: () => user.roles = [...rolestoUpdate.roles]
          })
        }
      }
    });
  }

  getRolesArray(user: User) {
    const roles = [];
    const userRoles = user.roles;
    const availableRoles: any[] = [
      {name: 'Admin', value: 'Admin'},
      {name: 'Member', value: 'Member'},
      {name: 'Moderator', value: 'Moderator'}
    ];

    availableRoles.forEach(role => {
      let match = false;
      for (const userRole of userRoles) {
        if (role.name === userRole) {
          match = true;
          role.checked = true;
          roles.push(role);
          break;
        }
      }
      if (!match) {
        role.checked = false;
        roles.push(role);
      }
    });
    return roles;
  }

}
