import * as React from 'react'

import { RouteComponentProps } from 'react-router-dom';
import { apiConfig, getDefaultHeaders, msalClient } from '../../store/AuthStore';
import UserData from '../Models/UserData';
import { Button } from 'react-bootstrap';
import PartnerRequestData from '../Models/PartnerRequestData';
import PartnerRequestStatusData from '../Models/PartnerRequestStatusData';
import { getPartnerRequestStatus } from '../../store/partnerRequestStatusHelper';
import * as Constants from '../Models/Constants'

interface AdminPartnerRequestsPropsType extends RouteComponentProps {
    partnerRequestList: PartnerRequestData[];
    partnerRequestStatusList: PartnerRequestStatusData[];
    isPartnerRequestDataLoaded: boolean;
    onPartnerRequestListChanged: any;
    isUserLoaded: boolean;
    currentUser: UserData;
};

export const AdminPartnerRequests: React.FC<AdminPartnerRequestsPropsType> = (props) => {

    // Handle approve request for a partner  
    function handleApprove(id: string, name: string) {
        if (!window.confirm("Do you want to approve partner with name: " + name))
            return;
        else {
            const account = msalClient.getAllAccounts()[0];

            var request = {
                scopes: apiConfig.b2cScopes,
                account: account
            };

            msalClient.acquireTokenSilent(request).then(tokenResponse => {
                const headers = getDefaultHeaders('PUT');
                headers.append('Authorization', 'BEARER ' + tokenResponse.accessToken);

                fetch('api/partnerrequests/approve/' + id, {
                    method: 'put',
                    headers: headers
                }).then(() => { props.onPartnerRequestListChanged(); });
            });
        }
    }

    // Handle approve request for a partner  
    function handleDeny(id: string, name: string) {
        if (!window.confirm("Do you want to deny partner with name: " + name))
            return;
        else {
            const account = msalClient.getAllAccounts()[0];

            var request = {
                scopes: apiConfig.b2cScopes,
                account: account
            };

            msalClient.acquireTokenSilent(request).then(tokenResponse => {
                const headers = getDefaultHeaders('PUT');
                headers.append('Authorization', 'BEARER ' + tokenResponse.accessToken);

                fetch('api/partnerrequests/deny/' + id, {
                    method: 'put',
                    headers: headers
                }).then(() => { props.onPartnerRequestListChanged(); });
            });
        }
    }

    function renderPartnerRequestsTable(partnerRequests: PartnerRequestData[]) {
        return (
            <div>
                <table className='table table-striped' aria-labelledby="tableLabel">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Email</th>
                            <th>Phone</th>
                            <th>Website</th>
                            <th>City</th>
                            <th>Region</th>
                            <th>Country</th>
                            <th>Request Status</th>
                            <th>Is Become Partner Request</th>
                            <th>Notes</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {partnerRequests.map(partnerRequest => {
                            return (
                                <tr key={partnerRequest.id.toString()}>
                                    <td>{partnerRequest.name}</td>
                                    <td>{partnerRequest.email}</td>
                                    <td>{partnerRequest.phone}</td>
                                    <td>{partnerRequest.website}</td>
                                    <td>{partnerRequest.city}</td>
                                    <td>{partnerRequest.region}</td>
                                    <td>{partnerRequest.country}</td>
                                    <td>{getPartnerRequestStatus(props.partnerRequestStatusList, partnerRequest.partnerRequestStatusId)}</td>
                                    <td>{partnerRequest.isBecomeAPartnerRequest}</td>
                                    <td>{partnerRequest.notes}</td>
                                    <td>
                                        <Button hidden={!partnerRequest.isBecomeAPartnerRequest || partnerRequest.partnerRequestStatusId !== Constants.PartnerRequestStatusSent } className="action" onClick={() => handleApprove(partnerRequest.id, partnerRequest.name)}>Approve Partner</Button>
                                        <Button hidden={!partnerRequest.isBecomeAPartnerRequest || partnerRequest.partnerRequestStatusId !== Constants.PartnerRequestStatusSent} className="action" onClick={() => handleDeny(partnerRequest.id, partnerRequest.name)}>Deny Partner</Button>
                                    </td>
                                </tr>)
                        }
                        )}
                    </tbody>
                </table>
            </div>
        );
    }

    let contents = props.isPartnerRequestDataLoaded
        ? renderPartnerRequestsTable(props.partnerRequestList)
        : <p><em>Loading...</em></p>;

    return (
        <div>
            <h1 id="tableLabel">Partner Requests</h1>
            {contents}
        </div>
    );
}

