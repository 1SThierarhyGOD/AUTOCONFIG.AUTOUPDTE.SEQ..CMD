import { Injectable } from '@angular/core';
import { UtilService } from './util.service';
import { environment } from '@app-env/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(private util: UtilService) { }

  // Determine if a user is Authenticated
  public get isAuthenticated(): boolean {

    const userCookie = this.util.getCookieByName(environment.jwtCookieName);

    // The JWT cookie is provided by the server during the SAML authentication process, 
    // so if it's present then the user is authenticated.
    return (userCookie != null && userCookie != undefined);
  }

}
