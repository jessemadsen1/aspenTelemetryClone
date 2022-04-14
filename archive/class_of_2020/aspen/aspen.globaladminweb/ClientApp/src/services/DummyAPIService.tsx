import { IAPIService } from "./IAPIService"
import { IDomainService } from "./IDomainService";
import { CharityHomePage } from "../models/CharityHomePageModel";
import { Charity } from "../models/CharityModel";
import { Theme } from "../models/Theme";
import {DomainService} from "./DomainService"
import { Team } from "../models/TeamModel";

export class DummyAPIService implements IAPIService{
    IDomainService: IDomainService = new DomainService();
    public async GetCharityHomePage(): Promise <CharityHomePage> {
        let theme = new Theme("#438f00","#67cc0e","#FFFFFF", "#608045","Arial");
        let charity = new Charity("89e0a4d3-f42c-4479-af22-2a3cba6bff8a","Kylers penguin's","kyler.com","this is where the awesome penguin's live")
        let charityHomePage = new CharityHomePage(theme,charity);
        
        return charityHomePage;
    }
    public async GetAllCharities(): Promise<Charity[]> {
        let charity = new Charity("89e0a4d3-f42c-4479-af22-2a3cba6bff8a","Kylers penguin's","kyler.com","this is where the awesome penguin's live")
        return [charity]
    }
    public async GetCharityByID(ID: string): Promise<Charity> {
        if(ID == "89e0a4d3-f42c-4479-af22-2a3cba6bff8a"){
            let charity = new Charity("89e0a4d3-f42c-4479-af22-2a3cba6bff8a","Kylers penguin's","kyler.com","this is where the awesome penguin's live")
            return charity
        }else{
            return new Charity("89e0a4d3-f42c-4479-af22-2a3cba6bff8a","","","")
        }
    }
    public async PostCreateCharity(charity : Charity): Promise<boolean> {
        return true
    }
    public async PostUpdateCharity(charity: Charity): Promise<boolean> {
        return true
    }
    public async PostDeleteCharity(charity: Charity): Promise<boolean> {
        return true
    }
    public async PostUpdateTeam(team: Team, charityId: string): Promise<boolean> {
        return true;
    }
    public async PostCreateTeam(team: Team, charityId: string): Promise<boolean> {
        return true;
    }
    GetTeamByCharityID(charityId: string): Promise<Team[]> {
        throw new Error("Method not implemented.");
    }
}