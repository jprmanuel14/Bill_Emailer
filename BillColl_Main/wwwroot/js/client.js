let clientEmailSort = "";
let clientContactSort = "";
let partnerSort = "";
let pageNumber = 0;
let pageSize = 10;
let clientName = "";
let clientEmail = "";
let clientContact = "";
let groupSearch = "";
let groupSearchSort = "";
let partner = "";
let SortHierarchy = [];
let edited = false

let qparams = new URLSearchParams(window.location.search)
let client_main_table = document.getElementById('tables')
let client_detail = document.getElementById('clientdetail')
let tab = 1;
let qstr = qparams.get('reference');
let groupcode  = ''

const checkIfValidContact = new RegExp(/^\d+$/, 'g');

//reasonDropDown move to exception

$('#addUserModal').on('hidden.bs.modal', function () {
    // do something…

    FName = ''
    document.getElementById('FnameInput').value = ''
    MName = ''
    document.getElementById('MnameInput').value = ''
    LName = ''
    document.getElementById('LnameInput').value = ''
    Salutation = ''
    document.getElementById('salutationInput').value = ''
    Email = ''
    document.getElementById('emailInput').value = ''
    Designation = ''
    document.getElementById('designationInput').value = ''
    designated_LOS = ''
    document.getElementById('designatedLos').value = ''
    GroupCode = ''
    document.getElementById('ContactNumberInput').value = ''
    ContactNumber = ''
    document.getElementById('myInput').value = 'Select group'

});

$('#editUserModal').on('hidden.bs.modal', function () {
    // do something…

    FName = ''
    MName = ''
    LName = ''
    document.getElementById('FnameInputEdit').value = ''
    document.getElementById('MnameInputEdit').value = ''
    document.getElementById('LnameInputEdit').value = ''
    Salutation = ''
    document.getElementById('salutationInputEdit').value = ''
    Email = ''
    document.getElementById('emailInputEdit').value = ''
    Designation = ''
    document.getElementById('designationInputEdit').value = ''
    designated_LOS = ''
    document.getElementById('designatedLosEdit').value = ''
    GroupCode = ''
    document.getElementById('myInputEdit').value = 'Select group'


    document.getElementById('updateButton').classList.add('disableBtn')
    document.getElementById('updateButton').disabled = true


});

$('#addUserEngagementModal').on('hidden.bs.modal', function () {
    // do something…

    document.getElementById('engagementInputEmail').value = ''
    document.getElementById('engagementInputDesignation').value = ''
    document.getElementById('engagementInputGroup').value = ''
    document.getElementById('myInputEngagementUser').value = ''


});

$('#editUserEngagementModal').on('hidden.bs.modal', function () {
    // do something…

    document.getElementById('engagementInputEmail').value = ''
    document.getElementById('engagementInputDesignation').value = ''
    document.getElementById('engagementInputGroup').value = ''
    document.getElementById('myInputEngagementUser').value = ''


});




let validEmail = false;


//selectedClientContact
let selectedContactClient = ''
let selectedEngagementContact = ''

let groupCodeList = []
let groupCodeListUmuttable = []
let ClientRef = ''

let modalState = ''


//addingUserForm and EditForm

let CCode = ''
let Name = ''
let FName = ''
let MName = ''
let LName = ''
let ContactNumber = ''
let Salutation = ''
let Email = ''
let Designation = ''
let Group = ''
let GroupCode = ''
let GroupType = ''
let LOS = ''
let EmployeeCode = null
let ClientCode = ''


//adding EngagementUserForm and EditForm
let emailEngagement = ''
let designationEngagement = ''
let groupEngagement = ''
let EName = ''
let EEcode = ''
let groupId = ''


//persistData
let P_reasonVal = ''
let P_other_enable = false
let P_other_reason = ''
let P_selectedClient = ''
let P_updatedReason = ''
let P_updatedOtherReason = ''


//addToException
let reasonVal = ''
let other_enable = false
let other_reason = ''
let selectedClient = ''
let updatedReason = ''
let updatedOtherReason = ''
let invoice_Number = ''
let bill_Number = ''
//removeToException


let logsList = []
let unmuttableLogsList = []


let exceptionID = ''

//for clienet tab
let dateSortPrevClient = "";
let pageNumberClient = 0;
let pageSizeClient = 10;
let dateSentPrevClient = "";


//for exception tab
let dateSortPrevException = "";
let pageNumberException = 0;
let pageSizeException = 10;
let dateSentPrevException = "";
let clientNameException = $('#RefClientName').val();


let downloadClient = '/Report/DownloadClientList'
let downloadException = '/Report/DownloadExceptions'

document.addEventListener('click', function handleClickOutsideBox(event) {
    // 👇️ the element the user clicked
    
    let target = event.target.classList.value
    let onClickElementByClass = document.getElementsByClassName('my-tooltip')
    let but = document.getElementById('button')
  
    if (target.includes('Appkit4-icon') || target.includes('my-tooltip-item') && !target.includes('changeReason')) {

        //
    }
    //else if (target.includes('changeReason')) { 

    //    let id = event.target
    //    
    //}
    else {
        for (i = 0; i < onClickElementByClass.length; i++) {
            /*        */
            onClickElementByClass[i].style.display = "none"
        }
    }



});

if (qstr == null) {

    const myparamC = pageNumber + "|" + pageSize + "|" + clientName + "|" + clientContact + "|" + clientEmail + "|" + partner + "|" + clientContactSort + "|" + clientEmailSort + "|" + partnerSort + "|" + groupSearch + "|" + groupSearchSort;
    loadClientList('/Clients/GetClients', myparamC, '#tblBodyClients', '#tblFooterClients', '.c-pagination', SortHierarchy);

} else {
    $.get('/Clients/GetCurrentPageSize?Reference=' + qstr, function (data) {

        pageNumber = data[3];

        $('#pageNumber').val(data[3]);
        $('#pageSize').val(data[4]);

        if (typeof (data[5]) != 'undefined') {
            clientName = data[5];
            $('#RefClientName').val(data[5]);
            $('#SearchText').val(data[5]);
        }
        const myparamC = pageNumber + "|" + pageSize + "|" + clientName + "|" + clientContact + "|" + clientEmail + "|" + partner + "|" + clientContactSort + "|" + clientEmailSort + "|" + partnerSort + "|" + groupSearch + "|" + groupSearchSort;
        loadClientList('/Clients/GetClients', myparamC, '#tblBodyClients', '#tblFooterClients', '.c-pagination', SortHierarchy);

    });

}


// let clientNameException = $('#RefClientName').val();

function loadClientList(url, param, tblbody, tblfooter, pagination_class, mySort) {

        

    document.getElementById('footer').classList.add('d-none')
    document.getElementById('footer').classList.remove('d-flex')
    let clientstr = "";
    let myparamlist = String(param);
    let myparam = myparamlist.split("|");



    let totalpages = 0;
    let currentpage = 0;
    let currentrowmaxdata;
    const maxDisplayRange = 5;


    document.getElementById('selectedClient').innerHTML = ""


    $(tblbody).empty();
    $(tblfooter).empty();

    let sortH = "";
    mySort.forEach(function (item, index) {
        sortH += item + "|";
    });

    sortH = sortH.substring(0, sortH.length - 1);

    $.getJSON(url, {
        pageNumber: (pageNumberClient * 10), pageSize: myparam[1], clientName: myparam[2], clientContact: myparam[3], clientEmail: myparam[4]
        , partner: myparam[5], clientContactSort: myparam[6], clientEmailSort: myparam[7], partnerSort: myparam[8]
        , groupSearch: myparam[9], groupSearchSort: myparam[10]
        , sortH: sortH

    }, function (data) {



        const interval = myparam[0];
        if (data.data.length != 0) {
            totalpages = data.data[0].totalPages;
        }
        else {
            totalPages = 0
        }

     


        //currentpage = pageNumberClient;
        //currentrowmaxdata = (data.currentPage * interval);

        //let frowname = '';
        //let lrowname = '';
        //let totalname = '';

        //frowname = '#firstrowC';
        //lrowname = '#lastrowC';
        //totalname = '#totaldataC';


        //if (totalpages > 0) {
        //    if (currentpage == 1) {
        //        $(frowname).text(currentpage);
        //    } else {
        //        $(frowname).text((parseInt(currentrowmaxdata) - parseInt(interval)) + 1);
        //    }

        //    if (currentpage == totalpages) {
        //        $(lrowname).text(data.totalData);
        //    } else {

        //        $(lrowname).text(parseInt(currentrowmaxdata));
        //    }
        //} else {
        //    $(frowname).text(0);
        //    $(lrowname).text(0);
        //}

        //$(totalname).text(data.totalData);


        if (data != '') {




            let ctr = 1;
            data.data.forEach((d) => {
                let finalVal = JSON.stringify(d)

                console.log('totalpages ' + d);
                console.log(finalVal);

                clientstr += '<tr>'
                          + '<td style="width:130px; height:50px;">'
                          +   `<div  class="btn btn-td-link"   onclick='ShowDebtorInfo(this, ${finalVal})'>`
                          +    '<input type="hidden" class="refNo" value="'+escapeHtml(d.eClientId)+'">  ' + escapeHtml(d.clientId) + '</div>'
                          + '</td>'

                          + '<td style="width:350px; height:50px;">'
                          + `<div  class="btn btn-td-link" onclick='ShowDebtorInfo(this, ${finalVal})'> ${escapeHtml(d.clientName)} </div>`
                          + '</td>'

                        + '<td style="height:50px;">'
                        + `<div  class="btn btn-td-link" onclick='ShowDebtorInfo(this, ${finalVal})'"> ${escapeHtml(d.billNo)} </div>`
                        + '</td>'

                          + '<td style="height:50px;">'
                          + `<div  class="btn btn-td-link" onclick='ShowDebtorInfo(this, ${finalVal})'> ${escapeHtml(d.reference_No)} </div>`
                          + '</td>'
                          
                         

                          + '<td style="height:50px;">'
                          + `<div  class="btn btn-td-link" onclick='ShowDebtorInfo(this, ${finalVal})'> ${escapeHtml(d.groupName)} </div>`
                          + '</td>'

                          + '<td style="height:50px;">'
                          + `<div  class="btn btn-td-link" onclick='ShowDebtorInfo(this, ${finalVal})'> ${escapeHtml(d.contactEmail)} </div>`
                          + '</td>'

                          + '<td style="width:10px;">'
                          + '<div style="position: relative;" class="d-flex justify-content-center">'
                          + '<div class="togglePanel" style="display:none;">'
                          + '<div>'

                          + `<button class="btn btn-toggle-custom" onclick='return MoveClientToException(this);'>Move to exception list</button>`
                          + '</div></div>'
                          + `<button id="toggle" class="btn" onclick='moveToException(${escapeHtml(finalVal)})'> <span class="Appkit4-icon icon-edit-outline icon-hover"></span> </button> </div> </td> </tr>`
                 
            

            })


            let val = escapeHtml(clientstr)
            $(tblbody).append(unescapeHtml(val)).html();
          

        }

        document.getElementById('footer').classList.add('d-flex')
        document.getElementById('footer').classList.remove('d-none')
        //Footer tblfooter
        $(tblfooter).empty();
        if (totalpages <= 0) {
            var rowval = "";
            rowval = "<td colspan='4'><div class='w-100 d-flex justify-content-center'>No data in the list</div></td>";
            row = "<tr>" + rowval + "</tr>";

            let val = escapeHtml(row)
            $(tblfooter).append(unescapeHtml(val));
        }

        //Pagination
        $(pagination_class).empty();
        let url = '/MyPage?list=';
        let myli = '';

        if (pageNumberClient + 1 == 1) {
            myli += "<li class='page-item mx-1 disable' style='border:0'><span class='page-link'>" + "Previous" + "</span></li>"
        } else {
            myli += `<li class='page-item  mx-1' style='border:0'><a class='page-link' onclick='navigatePageClient("${pageNumberClient - 1}")'>Previous</a></li>`
        }

        let startPage;
        let endPage;

      

        myli += `
        <div class='d-flex align-items-center justify-content-center'>
                    <div class='currentPage-container'>
                     <span>${pageNumberClient + 1}</span>
                   </div>
                   <span class="ms-2">of</span>
                   <span class="ms-2">${totalpages}</span>
             </div>
     `

        if (pageNumberClient + 1 == totalpages || totalpages == 0) {
            myli += "<li class='page-item mx-1 disable' style='border:0'><span class='page-link'>" + "Next" + "</span></li>"
        } else {
            myli += `<li class='page-item  mx-1' style='border:0'><button onclick='navigatePageClient("${pageNumberClient + 1}")' id='exceptionNext' class='page-link' >Next</button></li>`
        }
        let val = escapeHtml(myli)
        $(pagination_class).append(unescapeHtml(val));
    });
}



const loadActivityLogs = () => {
    //tblBodyLogs

    $('#tblBodyLogs').empty();
    $('#activityLogsModal').modal('show');

    let searchVal = document.getElementById('searchVal').value = ''

    $.getJSON('/viewlogreport_clientengagement', {}, (res) => {

        logsList = res
        unmuttableLogsList = res

        res.map((i, k) => {
            let htmlData = `
                       <tr>
                            <td>
                                <div class="p-3">
                                  
                                    ${moment(i.date_Log).format('DD MMMM YYYY')}
                                </div>    
                            </td>

                              <td>
                                <div class="p-3">
                                    ${moment(i.time_Log).format('HH:mm')}
                                </div>    
                            </td>

                              <td>
                                <div class="p-3">
                                   ${i.user_Name}
                                </div>    
                            </td>

                              <td>
                                <div class="p-3">
                                    ${i.client_Name}
                                </div>    
                            </td>

                            <td>
                                <div class="p-3">
                                    ${i.invoice_Number}
                                </div>    
                            </td>
                            <td>
                            <div class="p-3">
                                ${i.bill_No}
                            </div>    
                        </td>

                              

                              <td>
                                <div class="p-3">
                                    ${i.action}
                                </div>    
                            </td>

                              <td>
                                <div class="p-3 reason-ellipsis">
                                    ${i.previous_Exception_Reason}
                                </div>    
                            </td>

                              <td>
                                <div class="p-3 reason-ellipsis">
                                    ${i.current_Exception_Reason}
                                </div>    
                            </td>

                             <td>
                                <div class="p-3 reason-ellipsis">
                                    ${i.deletion_Reason}
                                </div>    
                            </td>
                       </tr>
                   `
            let val = escapeHtml(htmlData)
            $(tblBodyLogs).append(unescapeHtml(val)).html();
        })


    })



}




const loadException = () => {




    //let dateSortPrev = "";
    //let pageNumber = 1;
    //let pageSize = 10;
    //let dateSentPrev = "";
    //let clientName = $('#RefClientName').val();

    const myparam = (pageNumberException * 10) + "|" + pageSizeException + "|" + clientNameException + "|" + dateSentPrevException + "|" + dateSortPrevException;
    loadExceptionList('/Settings/GetData', myparam, '#tblBodyExceptions', '#tblFooterExceptions', '.footerexception');


}

function loadExceptionList(url, param, tblbody, tblfooter, pagination_class) {

    document.getElementById('footer').classList.add('d-none')
    document.getElementById('footer').classList.remove('d-flex')
    let utilstr = "";
    let myparamlist = String(param);
    let myparam = myparamlist.split("|");

    let totalpages = 0;
    let currentpage = 0;
    let currentrowmaxdata;
    const loadClientList = 5;


    $(tblbody).empty();
    $(tblfooter).empty();

    $.getJSON(url, {
        pageNumber: myparam[0], pageSize: myparam[1], clientName: myparam[2], dateSentPrev: myparam[3], dateSortPrev: myparam[4]

    }, function (data) {

        const interval = myparam[0];

        if (data.data.length != 0) {
            totalpages = data.data[0].totalPages
        }
        else {
            totalPages = 0
        }


        //currentpage = data.currentPage;
        //currentrowmaxdata = (data.currentPage * interval);

        //let frowname = '';
        //let lrowname = '';
        //let totalname = '';

        //frowname = '#firstrowUtil';
        //lrowname = '#lastrowUtil';
        //totalname = '#totaldataUtil';


        //if (totalpages > 0) {
        //    if (currentpage == 1) {
        //        $(frowname).text(currentpage);
        //    } else {
        //        $(frowname).text((parseInt(currentrowmaxdata) - parseInt(interval)) + 1);
        //    }

        //    if (currentpage == totalpages) {
        //        $(lrowname).text(data.totalData);
        //    } else {

        //        $(lrowname).text(parseInt(currentrowmaxdata));
        //    }
        //} else {
        //    $(frowname).text(0);
        //    $(lrowname).text(0);
        //}

        //$(totalname).text(data.totalData);
        if (data != '') {
            //
            let ctr = 1;
            data.data.forEach((d, index) => {

                let finalVal = JSON.stringify(d)

                console.log('sadhguru: ' + d);
                console.log('sadhguru - finalVal: ' + finalVal);

                //utilstr += '<tr><td><a href="" class="btn" onclick="return ShowDebtorInfo(this);"><input type="hidden" class="refNo" value="' + d.eClientId + '">' + d.clientId + '</td><td><a href="" class="btn" onclick="return ShowDebtorInfo(this);">' + d.clientName + '</a></td><td><a href="" class="btn" onclick="return ShowDebtorInfo(this);">' + d.contactEmail + '</a></td><td> <div style="position: relative;" class="d-flex justify-content-end"><div class="togglePanel" style="display:none;"><div><button class="btn btn-toggle-custom" onclick="return MoveClientToException(this);">Move to exception list</button></div></div><button id="toggle" class="btn" onclick="return toggleMenu(this);"><div class="burgerMenu-icon"></div></button></div></td></tr>';

                utilstr += `<tr>
                                <td>
                                    <div class="btn btn-td-link" onclick='ShowExceptionInfo(${finalVal})'><input type="hidden" class="refNo" value="' + d.eId + '"><input type="hidden" class="refReasons" value="' + d.reason + '">${d.clientCode}</div>
                                </td>
                                <td>
                                    <div class="btn btn-td-link" onclick='ShowExceptionInfo(${finalVal})'>${d.clientName}</div>
                                </td>

                                <td>
                                <div class="btn btn-td-link" onclick='ShowExceptionInfo(${finalVal})'>${d.bill_No == null ? "" : d.bill_No}</div>
                                </td>


                                 <td>
                                    <div class="btn btn-td-link" onclick='ShowExceptionInfo(${finalVal})'>${d.reference_No == null ? "" : d.reference_No}</div>
                                </td>

                                <td style="display:none;">
                                    <div onclick='ShowExceptionInfo(${finalVal})'>${d.group_Code == null ? "" : d.group_Code}</div>
                                </td>
                                
                             
                            

                                <td>
                                    <span>${d.groupName}</span>
                                </td> 
                                

                                 <td>
                                    <span>${d.contact_Email}</span>
                                </td>

                                 <td>
                                    <span>${d.previousSentDate == null ? "" : d.previousSentDate}</span>
                                </td>

                               
                                <td>
                                    <div style="position: relative;" class="d-flex justify-content-center"><div class="togglePanel" style="display:none;">
                                        <div>
                                            <button class="btn btn-toggle-custom" onclick="return RemoveClientToException(this);">Remove</button>
                                        </div>
                                    </div>
                                         <div id="toggle" class="btn" onclick='return toggleMenu(${finalVal}, "${index}")';>
                                            <span class="Appkit4-icon icon-vertical-more-outline"></span>

                                                   <div  id='${"tooltip" + d.clientName + index}' class='my-tooltip position-absolute  flex-column align-items-start' style='width: 250px; text-align: left; '>

                                                                           <div id='${"tooltip" + d.clientName + index}' onclick='changeReason(event, ${finalVal}, ${index})' class='my-tooltip-item changeReason'>
                                                                                Change reason of exception
                                                                           </div>
                                                                            <div id='${"tooltip" + d.clientName + index}' onclick='removeFromException(event, ${finalVal}, ${index})' class='my-tooltip-item changeReason'>
                                                                                Remove from exception list
                                                                           </div>
                                                                    
                                                                 </div>
                                         </div></div>
                                 </td>
                             </tr>`;

            })
            let val = escapeHtml(utilstr)
            $(tblbody).append(unescapeHtml(val));

        }
        //else {
        //    var rowval = "";
        //    rowval = "<td colspan='4'><div class='w-100 d-flex justify-content-center'>No exception list</div></td>";
        //    row = "<tr>" + rowval + "</tr>";

        //    let val = escapeHtml(row)
        //    $(tblfooter).append(unescapeHtml(val));
        //}

        document.getElementById('footer').classList.add('d-flex')
        document.getElementById('footer').classList.remove('d-none')
        //Footer tblfooter
        $(tblfooter).empty();

        if (totalpages <= 0) {
            var rowval = "";
            rowval = "<td colspan='4'><div class='w-100 d-flex justify-content-center'>No exception list</div></td>";
            row = "<tr>" + rowval + "</tr>";
            
            let val = escapeHtml(row)
            $(tblfooter).append(unescapeHtml(val));
        }

        //Pagination
        $(pagination_class).empty();
        let url = '/MyPage?list=';
        let myli = '';



        if (pageNumberException + 1 == 1) {
            myli += "<li class='page-item mx-1 disable' style='border:0'><span class='page-link'>" + "Previous" + "</span></li>"
        } else {
            myli += `<li class='page-item  mx-1' style='border:0'><a class='page-link' onclick='navigatePageException("${pageNumberException - 1}")'>Previous</a></li>`
        }

        let startPage;
        let endPage;

        //if (totalpages <= maxDisplayRange) {
        //    startPage = 1;
        //    endPage = totalpages;
        //} else {
        //    let maxPagesBeforeCurrentPage = Math.floor(maxDisplayRange / 2);
        //    let maxPagesAfterCurrentPage = Math.ceil(maxDisplayRange / 2) - 1;

        //    if (currentpage <= maxPagesBeforeCurrentPage) {
        //        // current page near the start
        //        startPage = 1;
        //        endPage = maxDisplayRange;
        //    } else if (currentpage + maxPagesAfterCurrentPage >= totalpages) {
        //        // current page near the end
        //        startPage = totalpages - maxDisplayRange + 1;
        //        endPage = totalpages;
        //    } else {
        //        // current page somewhere in the middle
        //        startPage = currentpage - maxPagesBeforeCurrentPage;
        //        endPage = currentpage + maxPagesAfterCurrentPage;
        //    }
        //}

        //for (let i = startPage; i <= endPage; i++) {
        //    if (i == currentpage) {
        //        myli += "<li class='page-item active mx-1'><span class='page-link'>" + i + "</span></li>"
        //    } else {
        //        myli += "<li class='page-item  mx-1'><a class='page-link' href='" + url + i + "'>" + i + "</a></li>"
        //    }
        //}

        myli += `
                   <div class='d-flex align-items-center justify-content-center'>
                               <div class='currentPage-container'>
                                <span>${totalpages <= 0 ? 0 : pageNumberException + 1}</span>
                              </div>
                              <span class="ms-2">of</span>
                              <span class="ms-2">${totalpages}</span>
                        </div>
                `


        if (pageNumberException + 1 == totalpages || totalpages == 0) {
            myli += "<li class='page-item mx-1 disable' style='border:0'><span class='page-link'>" + "Next" + "</span></li>"
        } else {
            myli += `<li class='page-item  mx-1' style='border:0'><button onclick='navigatePageException("${pageNumberException + 1}")' id='exceptionNext' class='page-link' >Next</button></li>`
        }

        let val = escapeHtml(myli)
        $(pagination_class).append(unescapeHtml(val));
    });
}

const navigatePageException = (page) => {


    pageNumberException = parseInt(page)



    loadException()


}


const navigatePageClient = (page) => {


    pageNumberClient = parseInt(page)

    const myparamC = pageNumber + "|" + pageSize + "|" + clientName + "|" + clientContact + "|" + clientEmail + "|" + partner + "|" + clientContactSort + "|" + clientEmailSort + "|" + partnerSort + "|" + groupSearch + "|" + groupSearchSort;
    loadClientList('/Clients/GetClients', myparamC, '#tblBodyClients', '#tblFooterClients', '.c-pagination', SortHierarchy);

    // loadClientList()


}


var $selectR = $('#GroupName').selectize({
    create: false,
    sortField: 'text'
});


const switchTab = (v) => {

    document.getElementById('RefClientName').value = ''
    clientNameException = ''

    if (v == 1) {
        //client list
        document.getElementById('exceptionTable').classList.add('d-none')
        document.getElementById('clientTable').classList.remove('d-none')


        document.getElementById('clientListTab').classList.add('active-tab')
        document.getElementById('exceptionListTab').classList.remove('active-tab')
        tab = 1;

        GetClientList();

    }
    else if (v == 2) {
        //exception list

        

        document.getElementById('clientTable').classList.add('d-none')
        document.getElementById('exceptionTable').classList.remove('d-none')

        document.getElementById('clientListTab').classList.remove('active-tab')
        document.getElementById('exceptionListTab').classList.add('active-tab')
        tab = 2;
        loadException()
    }
}

$(function () {





    getGroupCodeList()

    if (qstr != null) {
        //$('#ContactNameHeader').text(clientName);
        //$('#eClientCode').val(refno);
        $.get('/Clients/GetContactInfo?Reference=' + qstr, function (data) {
            //
            //

            PopulateCClist(qstr);
            PopulateEClist(qstr);

            $('#ContactNameHeader').text(data[0]);
            $('#eClientCode').val(qstr);

            if (data[2] == 'C') {
                $('#cContactsbtn').click();
            } else {
                $('#eContactsbtn').click();
            }

            $('#ModalUpdateContacts').modal('show');
        });
    }

    //$("#tblmyClients th").css("background-color", "white");
    //$("#tblmyClients th").css("position", "relative");
    //$("#tblmyClients th").css("z-index", "1000");
    //$("#tblmyClients th").css("border-bottom", "1px solid black");

    //var $th1 = $('.tableFixHeadC').find('thead th')
    //$('.tableFixHeadC').on('scroll', function () {
    //    $th1.css('transform', 'translateY(' + this.scrollTop + 'px)');
    //});


    //$("#tblmycContacts th").css("background-color", "white");
    //$("#tblmycContacts th").css("position", "relative");
    //$("#tblmycContacts th").css("z-index", "1000");
    //$("#tblmycContacts th").css("border-bottom", "1px solid black");

    //var $th2 = $('.tableFixHeadCC').find('thead th')
    //$('.tableFixHeadCC').on('scroll', function () {
    //    $th2.css('transform', 'translateY(' + this.scrollTop + 'px)');
    //});


    //$("#tblmyeContacts th").css("background-color", "white");
    //$("#tblmyeContacts th").css("position", "relative");
    //$("#tblmyeContacts th").css("z-index", "1000");
    //$("#tblmyeContacts th").css("border-bottom", "1px solid black");

    //var $th3 = $('.tableFixHeadEC').find('thead th')
    //$('.tableFixHeadEC').on('scroll', function () {
    //    $th3.css('transform', 'translateY(' + this.scrollTop + 'px)');
    //});


    $('#BtnSubmitReference').on('click', function () {

        const mySearchText = $('#RefClientName').val();

        

        $('#SearchText').val(mySearchText);

        if (tab == 1) {
            pageNumberClient = 0
            GetClientList();
        }
        else if (tab == 2) {
            clientNameException = mySearchText
            pageNumberException = 0
            loadException()
        }

        return false;
    });


    $(document).on('change', '#pageintervalC', function (event) {
        GetClientList();
    });



    $('#btnApplyFilterGroupSearch').on('click', function () {

        groupSearch = $('#GroupSearch').val();
        groupSearchSort = $('input[name="GroupSearchValue"]:checked').val();

        //if (typeof (groupSearchSort) != "undefined" && groupSearchSort != "") {
        //    if (!SortHierarchy.includes("GroupName")) {
        //        SortHierarchy.push("GroupName");
        //    }
        //}

        if (typeof (groupSearchSort) != "undefined" && groupSearchSort != "" || groupSearch != "") {
            $('#GroupSearchFilter').children().children('.filter-img').removeClass('disable');
            $('#GroupSearchFilter').children().children('.filter-img').addClass('active');
        } else {
            $('#GroupSearchFilter').children().children('.filter-img').removeClass('active');
            $('#GroupSearchFilter').children().children('.filter-img').addClass('disable');
        }

        $('#GroupSearchFilterPanel').hide();

        GetClientList();
        AddFilterC();
        return false;
    });


    $('#btnApplyFilterContactEmail').on('click', function () {

        clientEmail = $('#ContactEmail').val();
        clientEmailSort = $('input[name="ContactEmailListValue"]:checked').val();

        if (typeof (clientEmailSort) != "undefined" && clientEmailSort != "") {
            if (!SortHierarchy.includes("ContactEmail")) {
                SortHierarchy.push("ContactEmail");
            }
        }

        if (typeof (clientEmailSort) != "undefined" && clientEmailSort != "" || clientEmail != "") {
            $('#ContactEmailFilter').children().children('.filter-img').removeClass('disable');
            $('#ContactEmailFilter').children().children('.filter-img').addClass('active');
        } else {
            $('#ContactEmailFilter').children().children('.filter-img').removeClass('active');
            $('#ContactEmailFilter').children().children('.filter-img').addClass('disable');
        }

        $('#ContactEmailFilterPanel').hide();

        GetClientList();
        AddFilterC();
        return false;
    });

    //$('#btnApplyFilterPartner').on('click', function () {

    //    partner = $('#PartnerName').val();
    //    partnerSort = $('input[name="PartnerListValue"]:checked').val();

    //    if (typeof (partnerSort) != "undefined" && partnerSort != "") {
    //        if (!SortHierarchy.includes("PartnerInvolved")) {
    //            SortHierarchy.push("PartnerInvolved");
    //        }
    //    }

    //    if (typeof (partnerSort) != "undefined" && partnerSort != "" || partner != "") {
    //        $('#PartnerFilter').children().children('.filter-img').removeClass('disable');
    //        $('#PartnerFilter').children().children('.filter-img').addClass('active');
    //    } else {
    //        $('#PartnerFilter').children().children('.filter-img').removeClass('active');
    //        $('#PartnerFilter').children().children('.filter-img').addClass('disable');
    //    }

    //    $('#PartnerFilterPanel').hide();

    //    GetClientList();
    //    AddFilterC();
    //    return false;
    //});

    //$(document).on('click', '.c-pagination  a', function (event) {

    //    event.preventDefault();
    //    let mydatefrom, mydateto;

    //    var page = $(this).attr('href').split('list=')[1];
    //    //
    //    if (typeof page == 'undefined') {
    //        return;
    //    }

    //    $('.c-pagination li').removeClass('active');
    //    $(this).parent('li').addClass('active');

    //    pageNumber = page;

    //    const myparamC = pageNumber + "|" + pageSize + "|" + clientName + "|" + clientContact + "|" + clientEmail + "|" + partner + "|" + clientContactSort + "|" + clientEmailSort + "|" + partnerSort + "|" + groupSearch + "|" + groupSearchSort;
    //    loadClientList('/Clients/GetClients', myparamC, '#tblBodyClients', '#tblFooterClients', '.c-pagination', SortHierarchy);


    //});




    const nextPageClient = (page) => {

        $('.c-pagination li').removeClass('active');
        $(this).parent('li').addClass('active');

        pageNumber = page;

        const myparamC = pageNumber + "|" + pageSize + "|" + clientName + "|" + clientContact + "|" + clientEmail + "|" + partner + "|" + clientContactSort + "|" + clientEmailSort + "|" + partnerSort + "|" + groupSearch + "|" + groupSearchSort;
        loadClientList('/Clients/GetClients', myparamC, '#tblBodyClients', '#tblFooterClients', '.c-pagination', SortHierarchy);
    }




    function AddFilterC() {
        let myFilter = '';
        $('#myFiltersC').empty();

        if (clientContact != "" && typeof (clientContact) != "undefined") {
            myFilter += '<div class="filter-container"><div class="d-flex">Contact name: <div class="filter-items-2 ml-1" title="' + clientContact + '">' + clientContact + '</div><button id="removeclientContactFilter" class="btn filter-dispose-btn" onclick="removeFilter(this)">x</button></div></div>';
        }

        if (groupSearch != "" && typeof (groupSearch) != "undefined") {
            myFilter += '<div class="filter-container"><div class="d-flex">Group: <div class="filter-items-2 ml-1" title="' + groupSearch + '">' + groupSearch + '</div><button id="removegroupFilter" class="btn filter-dispose-btn" onclick="removeFilter(this)">x</button></div></div>';
        }


        if (clientContactSort != "" && typeof (clientContactSort) != "undefined") {
            myFilter += '<div class="filter-container"><div class="d-flex">Contact sort: <div class="filter-items-2 ml-1" title="' + clientContactSort + '">' + clientContactSort + '</div><button id="removeclientContactSortFilter" class="btn filter-dispose-btn" onclick="removeFilter(this)">x</button></div></div>';
        }

        if (clientEmail != "" && typeof (clientEmail) != "undefined") {
            myFilter += '<div class="filter-container"><div class="d-flex">Client email: <div class="filter-items-2 ml-1" title="' + clientEmail + '">' + clientEmail + '</div><button id="removeclientEmailFilter" class="btn filter-dispose-btn" onclick="removeFilter(this)">x</button></div></div>';
        }

        if (clientEmailSort != "" && typeof (clientEmailSort) != "undefined") {
            myFilter += '<div class="filter-container"><div class="d-flex">Client email sort: <div class="filter-items-2 ml-1" title="' + clientEmailSort + '">' + clientEmailSort + '</div><button id="removeclientEmailSortFilter" class="btn filter-dispose-btn" onclick="removeFilter(this)">x</button></div></div>';
        }

        if (partner != "" && typeof (partner) != "undefined") {
            myFilter += '<div class="filter-container"><div class="d-flex">Partner: <div class="filter-items-2 ml-1" title="' + partner + '">' + partner + '</div><button id="removepartnerFilter" class="btn filter-dispose-btn" onclick="removeFilter(this)">x</button></div></div>';
        }

        if (partnerSort != "" && typeof (partnerSort) != "undefined") {
            myFilter += '<div class="filter-container"><div class="d-flex">Partner sort: <div class="filter-items-2 ml-1" title="' + partnerSort + '">' + partnerSort + '</div><button id="removepartnerSortFilter" class="btn filter-dispose-btn" onclick="removeFilter(this)">x</button></div></div>';
        }


        if (groupSearchSort != "" && typeof (groupSearchSort) != "undefined") {
            myFilter += '<div class="filter-container"><div class="d-flex">Group sort: <div class="filter-items-2 ml-1" title="' + groupSearchSort + '">' + groupSearchSort + '</div><button id="removegroupSortFilter" class="btn filter-dispose-btn" onclick="removeFilter(this)">x</button></div></div>';
        }


        $('#myFiltersC').append(myFilter);
    }

    //auto search contact name
    //$('#ContactName').autocomplete({
    //    source: function (request, response) {

    //        var param = bindToken({ searchstr: $('#ContactName').val() });
    //        $.ajax({
    //            url: "/Clients/GetContactNameList",
    //            data: param,
    //            dataType: "json",
    //            type: "POST",
    //            contentType: "application/x-www-form-urlencoded",
    //            dataFilter: function (data) { return data; },
    //            success: function (data) {
    //                //
    //                response($.map(data, function (item) {
    //                    return {
    //                        label: item,
    //                        value: item
    //                    }
    //                }))

    //            },
    //            error: function (XMLHttpRequest, textStatus, errorThrown) {
    //                //var err = eval(XMLHttpRequest.responseText);
    //                alert(XMLHttpRequest.responseText)
    //            }
    //        });
    //    },
    //    minLength: 1 //This is the Char length of inputTextBox    
    //});

    //auto search contact email
    $('#ContactEmail').autocomplete({
        source: function (request, response) {

            var param = bindToken({ searchstr: $('#ContactEmail').val() });
            $.ajax({
                url: "/Clients/GetContactEmailList",
                data: param,
                dataType: "json",
                type: "POST",
                contentType: "application/x-www-form-urlencoded",
                dataFilter: function (data) { return data; },
                success: function (data) {
                    response($.map(data, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }))

                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    //var err = eval(XMLHttpRequest.responseText);
                    alert(XMLHttpRequest.responseText)
                }
            });
        },
        minLength: 1 //This is the Char length of inputTextBox    
    });

    //auto search group
    $('#GroupSearch').autocomplete({
        source: function (request, response) {

            var param = bindToken({ searchstr: $('#GroupSearch').val() });
            $.ajax({
                url: "/Clients/GetGroupList",
                data: param,
                dataType: "json",
                type: "POST",
                contentType: "application/x-www-form-urlencoded",
                dataFilter: function (data) { return data; },
                success: function (data) {
                    response($.map(data, function (item) {
                        return {
                            label: item,
                            value: item
                        }
                    }))

                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    //var err = eval(XMLHttpRequest.responseText);
                    alert(XMLHttpRequest.responseText)
                }
            });
        },
        minLength: 1 //This is the Char length of inputTextBox    
    });





    //auto search partner name
    //$('#PartnerName').autocomplete({
    //    source: function (request, response) {

    //        var param = bindToken({ searchstr: $('#PartnerName').val() });
    //        $.ajax({
    //            url: "/Clients/GetPartnerList",
    //            data: param,
    //            dataType: "json",
    //            type: "POST",
    //            contentType: "application/x-www-form-urlencoded",
    //            dataFilter: function (data) { return data; },
    //            success: function (data) {

    //                response($.map(data, function (item) {
    //                    return {
    //                        label: item,
    //                        value: item
    //                    }
    //                }))

    //            },
    //            error: function (XMLHttpRequest, textStatus, errorThrown) {
    //                //var err = eval(XMLHttpRequest.responseText);
    //                alert(XMLHttpRequest.responseText)
    //            }
    //        });
    //    },
    //    minLength: 1 //This is the Char length of inputTextBox    
    //});

    $('#BtnAddContact').on('click', function () {
        $('#rdClient').prop('disabled', false);
        $('#rdEngagement').prop('disabled', false);
        $('#eId').val('');

        $('#Email').val('');
        $('#FirstName').val('');
        $('#MiddleName').val('');
        $('#LastName').val('');
        $('#Salutation').val('');

        $('#ContactNumber').val('');
        $('#Name').val('');
        $('#Designation').val('');

        //$('#rdClient').click();

        if ($('#cContactsbtn').hasClass('btn-side-panel-deactive-2')) {
            $('#rdEngagement').click();
            $('#ClientP').addClass('d-none');
            $('.EngagementP').removeClass('d-none');

        } else {
            $('#rdClient').click();
            $('#ClientP').removeClass('d-none');
            $('.EngagementP').addClass('d-none');
        }

        //$('#ClientP').removeClass('d-none');
        //$('.EngagementP').addClass('d-none');




        $('#ModalAddNewContacts').modal('show');
    });

    $('#BtnDownload').on('click', function () {

        let data = clientName;

        // window.open("/Report/DownloadClientList?file=" + data, '_blank');
        if (tab == 1) {
            window.open(downloadClient)
        }
        else if (tab == 2) {
            window.open(downloadException)
        }
    });

    //$('#SubmitException').on('click', function () {
    //    let refno = $('#ExceptionRefno').val();
    //    let remark = $('#ExceptionReason').val();
    //    let myReference = bindToken({ reference: refno, remarks: remark });
    //    $.post('/Clients/MoveToException', myReference, function (a) {

    //        window.location.href = "/Clients";
    //    });


    //});

});

function ClearDisable() {
    $('#rdClient').prop('disabled', false);
    $('#rdEngagement').prop('disabled', false);
    //
    let iseng = $('input[name="IsEngagement"]:checked').val();

    if (iseng != 'True') {
        if ($('#eId').val() == null || $('#eId').val() == "") {
            if ($('#tblmycContacts tbody tr').length >= myClientContactLimit) {

                alert("You've exceeded the limit for adding of client contacts!");

                return false;
            }
        }
    }
    return true;
}


function GetClientList() {

    //pageSize = $('#pageintervalC').val();
    
    //clientName = $('#RefClientName').val();
    //clientContact = $('#ContactName').val();
    //clientEmail = $('#ContactEmail').val();
    //partner = $('#PartnerName').val();

    clientName = $('#RefClientName').val();
    clientContact = ''
    clientEmail = ''
    partner = ''

    

    const myparamC = pageNumber + "|" + pageSize + "|" + clientName + "|" + clientContact + "|" + clientEmail + "|" + partner + "|" + clientContactSort + "|" + clientEmailSort + "|" + partnerSort + "|" + groupSearch + "|" + groupSearchSort;
    loadClientList('/Clients/GetClients', myparamC, '#tblBodyClients', '#tblFooterClients', '.c-pagination', SortHierarchy);
}

function removeFilter(e) {

    $(e).parent().parent().remove();

    switch (e.id) {
        case "removeclientContactFilter":
            $('#ContactName').val('');

            $('#btnApplyFilterContactName').trigger('click');

            break;
        case "removegroupFilter":
            $('#GroupSearch').val('');

            $('#btnApplyFilterGroupSearch').trigger('click');

            break;

        case "removeclientContactSortFilter":

            $('input[name="ContactNameListValue"]').each(function () {
                this.checked = false;
            });

            let index1 = SortHierarchy.indexOf("ContactName");
            SortHierarchy.splice(index1, 1);

            $('#btnApplyFilterContactName').trigger('click');

            break;
        case "removeclientEmailFilter":
            $('#ContactEmail').val('');

            $('#btnApplyFilterContactEmail').trigger('click');

            break;
        case "removeclientEmailSortFilter":

            $('input[name="ContactEmailListValue"]').each(function () {
                this.checked = false;
            });

            let index2 = SortHierarchy.indexOf("ContactEmail");
            SortHierarchy.splice(index2, 1);

            $('#btnApplyFilterContactEmail').trigger('click');

            break;
        case "removepartnerFilter":
            $('#PartnerName').val('');

            $('#btnApplyFilterPartner').trigger('click');

            break;
        case "removepartnerSortFilter":
            $('input[name="PartnerListValue"]').each(function () {
                this.checked = false;
            });

            let index3 = SortHierarchy.indexOf("PartnerInvolved");
            SortHierarchy.splice(index3, 1);

            $('#btnApplyFilterPartner').trigger('click');

            break;
        case "removegroupSortFilter":
            $('input[name="GroupSearchValue"]').each(function () {
                this.checked = false;
            });

            //let index3 = SortHierarchy.indexOf("GroupName");
            //SortHierarchy.splice(index3, 1);

            $('#btnApplyFilterGroupSearch').trigger('click');

            break;

        default:
            break;
    }

}

const closeClientDetail = () => {
    client_main_table.classList.remove('d-none');
    client_detail.classList.add('d-none');

    const myparamC = pageNumber + "|" + pageSize + "|" + clientName + "|" + clientContact + "|" + clientEmail + "|" + partner + "|" + clientContactSort + "|" + clientEmailSort + "|" + partnerSort + "|" + groupSearch + "|" + groupSearchSort;
    loadClientList('/Clients/GetClients', myparamC, '#tblBodyClients', '#tblFooterClients', '.c-pagination', SortHierarchy);


}

    

//ShowDebtorInfo();

function ShowDebtorInfo(e, value) {

    
    
    

    let tr = $(e).closest('tr');
    let refno = tr.find('td:first-child .refNo').val();
    //let clientCode = tr.find('td:eq(0)').text();
    let clientName = tr.find('td:eq(1)').text();
    //let contactName = tr.find('td:eq(2)').text();

    //api for getting client contact and engagement contact
    
    ClientRef = value.clientId;
    invoice_Number = value.reference_No
    bill_Number = value.billNo
    groupcode = value.group_Code; 


    
    PopulateCClist(value.clientId);

    PopulateEClist(value.clientId);
    $('#pageNumber').val(pageNumber);
    $('#pageSize').val(pageSize);

    ClientCode = value.clientId

    document.getElementById('selectedClient').innerHTML = value.clientName;

    $('#ContactNameHeader').text(clientName);
    $('#eClientCode').val(value.clientId);
    //$('#ModalUpdateContacts').modal('show');


    client_main_table.classList.add('d-none');
    client_detail.classList.remove('d-none');

    return false;
}



function PopulateCClist(a) {
    $('#clienttabledetails').empty();
    $('#tblFooterCContacts').empty();

    $.getJSON('/Clients/GetCContactsLists?Reference=' + a, 'group_id_variable=' + groupcode, function (data) {

        if (data != '') {
            let myrow = '';


            //
            document.getElementById('clientnumberofrows').innerText = ''
            document.getElementById('clientnumberofrows').innerText = data.length


            if (data.length < 10) {

                document.getElementById('addUserButton').disabled = false
                document.getElementById('addUserButton').classList.remove('disableBtn')


            }
            else {
                document.getElementById('addUserButton').disabled = true
                document.getElementById('addUserButton').classList.add('disableBtn')


            }

            data.forEach((d) => {
                //myrow += `<tr><td><input type="hidden" class="refCId" value="' + d.eId + '"><input type="hidden" class="refFirstName" value="' + d.firstName + '"><input type="hidden" class="refMiddleName" value="' + d.middleName + '"><input type="hidden" class="refLastName" value="' + d.lastName + '"><input type="hidden" class="refDesignation" value="' + d.designation + "BAKET AYAW MO PO?" + + '"><input type="hidden" class="refSalutation" value="' + d.salutation + '">' + d.email + '</td><td>' + d.contactNumber + '</td><td> <div style="position: relative;" class="d-flex"><div class="togglePanel" style="display:none;"><div><button class="btn btn-toggle-custom" onclick="return EditCContact(this);">Update</button><button class="btn btn-toggle-custom" onclick="return RemoveCContact(this);">Delete</button></div></div><button id="toggle" class="btn" onclick="return toggleMenu(this);"><span class="Appkit4-icon icon-edit-outline"></span></button></div></td></tr>`

                let finalVal = JSON.stringify(d)
                myrow += `
                   <tr>
                        <td>
                            <div class="container p-3">
                                <span>${d.email}</span>
                            </div>
                        </td>
                        <td>
                            <div class="container p-3">
                                <span>${d.contactNumber}</span>
                            </div>
                        </td>
                        <td>
                            <div class="container p-3">
                                <span>${d.group_Description}</span>
                            </div>
                        </td>
                        <td>
                            <div class="container d-flex p-3">
                                <span onclick='openEditUser("${d.id}")' class="Appkit4-icon icon-pencil-fill  a-text-grey"></span>
                                <span onclick='deleteUser(${finalVal}, "client")' class="Appkit4-icon icon-delete-fill ms-2 a-text-grey"></span>
                            </div>
                        </td>
                    </tr>
                         `
            });
            let val = escapeHtml(myrow)
            $('#clienttabledetails').append(unescapeHtml(val));
        } else {
            document.getElementById('clientnumberofrows').innerText = 0
            document.getElementById('addUserButton').disabled = false
            document.getElementById('addUserButton').classList.remove('disableBtn')
            $('#tblFooterCContacts').append("<td colspan='4'><div class='w-100 d-flex justify-content-center'>No client contacts in the list</div></td>");
        }

        //
        if ($('#cContactsbtn').hasClass('btn-side-panel-deactive-2')) {
            $('#BtnAddContact').prop('disabled', false);
        } else {
            DisableAddContacts();

        }
    })
}

//create get ajax with two payloads




function PopulateEClist(a) {

    $('#engagementtabledetails').empty();
    $('#tblFooterEContacts').empty();

    
    // /Clients/GetEContactsLists?Reference=' + a

    $.getJSON('/display_engagement_team_list', {
        reference: a,
        //invoice_number_variable: invoice_Number,
        invoice_number_variable: bill_Number,
        group_code_variable: groupcode
    },function (data) {
        if (data != '') {
            let myrow = '';
            //

            document.getElementById('engagementnumberofrows').innerText = ''
            document.getElementById('engagementnumberofrows').innerText = data.length


            if (data.length < 10) {

                document.getElementById('addengagementbtn').disabled = false
                document.getElementById('addengagementbtn').classList.remove('disableBtn')


            }
            else {
                document.getElementById('addengagementbtn').disabled = true
                document.getElementById('addengagementbtn').classList.add('disableBtn')


            }

            data.forEach((d) => {

                let finalVal = JSON.stringify(d)
                myrow += `<tr>
                               
                               <td>
                                     <div class="container p3">
                                        ${d.name}
                                     </div>
                               </td>
                               <td> 
                                    
                                     <div class="container p3">
                                      ${d.email}
                                     </div>
                               </td>
                               
                               <td>
                                     <div class="container p3">
                                     ${d.designation}
                                     </div>
                               </td>
                               
                               <td> 
                                   ${d.isBillingManager == 0 ?  `<div class="container d-flex p-3">
                                        
                                   <span onclick='deleteUser(${finalVal}, "engagement")' class="Appkit4-icon icon-delete-fill ms-2 a-text-grey"></span>
                               </div>` : `<div class='p-3'></div>`}
                               </td>
                           </tr>`
            });

            //temporary remove 
            // <span onclick='openEditEngagementUser("${d.id}")' class="Appkit4-icon icon-pencil-fill  a-text-grey"></span>

            let val = escapeHtml(myrow)
            $('#engagementtabledetails').append(unescapeHtml(val));
        } else {
            document.getElementById('engagementnumberofrows').innerText = ''
            document.getElementById('engagementnumberofrows').innerText = 0
            document.getElementById('addengagementbtn').disabled = false
            document.getElementById('addengagementbtn').classList.remove('disableBtn')
            $('#tblFooterEContacts').append("<td colspan='4'><div class='w-100 d-flex justify-content-center'>No engagement team in the list</div></td>");
        }
    });
}

function EditCContact(e) {
    let tr = $(e).closest('tr');
    let refno = tr.find('td:first-child .refCId').val();
    let refFirstName = tr.find('td:first-child .refFirstName').val();
    let refMiddleName = tr.find('td:first-child .refMiddleName').val();
    let refLastName = tr.find('td:first-child .refLastName').val();
    let refDesignation = tr.find('td:first-child .refDesignation').val();
    let refSalutation = tr.find('td:first-child .refSalutation').val();

    $('#rdClient').prop('disabled', false);
    $('#rdEngagement').prop('disabled', false);


    $('#eId').val(refno);
    tr.find('td .togglePanel').hide();

    $('#rdClient').click();
    $('#rdClient').prop('disabled', true);
    $('#rdEngagement').prop('disabled', true);


    $('#ClientP').removeClass('d-none');
    $('.EngagementP').addClass('d-none');


    let email = tr.find('td:eq(0)').text();
    let contact = tr.find('td:eq(1)').text();

    $('#Email').val(email);
    $('#ContactNumber').val(contact);
    $('#FirstName').val(refFirstName);
    $('#MiddleName').val(refMiddleName);
    $('#LastName').val(refLastName);
    $('#Designation').val(refDesignation);
    $('#Salutation').val(refSalutation);


    $('#ModalAddNewContacts').modal('show');


}

function RemoveCContact(e) {
    let tr = $(e).closest('tr');
    let clientRef = $('#eClientCode').val()
    let refno = tr.find('td:first-child .refCId').val();

    $.get('/Clients/GetContactInfo?Reference=' + clientRef, function (data) {
        $.ajax({
            headers: {
                'X-CSRF-TOKEN': $('input[name="__RequestVerificationToken"]').val()
                , 'Content-Type': 'application/x-www-form-urlencoded'
            },
            type: 'DELETE',
            url: '/Clients/DeleteC/' + data[1] + '?Reference=' + refno,
            dataType: 'json',
            success: function (data) {
                PopulateCClist(data);


                const myparamC = pageNumber + "|" + pageSize + "|" + clientName + "|" + clientContact + "|" + clientEmail + "|" + partner + "|" + clientContactSort + "|" + clientEmailSort + "|" + partnerSort + "|" + groupSearch + "|" + groupSearchSort;
                loadClientList('/Clients/GetClients', myparamC, '#tblBodyClients', '#tblFooterClients', '.c-pagination', SortHierarchy);


            },
            error: function (xhr, status, error) {
                var errorMessage = xhr.status + ': ' + xhr.statusText
                alert('Error - ' + errorMessage);
            }
        });
    });


    tr.find('td .togglePanel').hide();
}

function EditEContact(e) {
    let tr = $(e).closest('tr');
    let refno = tr.find('td:first-child .refEId').val();
    let groupname = tr.find('td:first-child .refGroupName').val();
    $('#eId').val(refno);
    tr.find('td .togglePanel').hide();

    $('#rdClient').prop('disabled', false);
    $('#rdEngagement').prop('disabled', false);

    $('#rdEngagement').click();
    $('#rdClient').prop('disabled', true);
    $('#rdEngagement').prop('disabled', true);


    $('#ClientP').addClass('d-none');
    $('.EngagementP').removeClass('d-none');

    let email = tr.find('td:eq(1)').text();
    let contact = tr.find('td:eq(2)').text();
    let name = tr.find('td:eq(0)').text();
    let designated = tr.find('td:eq(3)').text();

    $('#Email').val(email);
    $('#ContactNumber').val(contact);
    $('#Name').val(name);
    $('#Designation').val(designated);

    var selectize = $selectR[0].selectize;

    selectize.setValue(groupname);

    $('#ModalAddNewContacts').modal('show');
}

function RemoveEContact(e) {


    let tr = $(e).closest('tr');
    let clientRef = e.clientCode
    let refno = e.id

    $.get('/Clients/GetContactInfo?Reference=' + clientRef, function (data) {

        $.ajax({
            headers: {
                'X-CSRF-TOKEN': $('input[name="__RequestVerificationToken"]').val()
                , 'Content-Type': 'application/x-www-form-urlencoded'
            },
            type: 'DELETE',
            url: '/Clients/' + data[0] + '?Reference=' + refno,
            dataType: 'json',
            success: function (data) {
                PopulateEClist(data);
                const myparamC = pageNumber + "|" + pageSize + "|" + clientName + "|" + clientContact + "|" + clientEmail + "|" + partner + "|" + clientContactSort + "|" + clientEmailSort + "|" + partnerSort + "|" + groupSearch + "|" + groupSearchSort;
                loadClientList('/Clients/GetClients', myparamC, '#tblBodyClients', '#tblFooterClients', '.c-pagination', SortHierarchy);

            },
            error: function (xhr, status, error) {
                var errorMessage = xhr.status + ': ' + xhr.statusText
                alert('Error - ' + errorMessage);
            }
        });
    });


    tr.find('td .togglePanel').hide();
}

function MoveClientToException(e) {

    
    let tr = $(e).closest('tr');
    let refno = tr.find('td:first-child .refNo').val();
    let clientName = tr.find('td:eq(1)').text();

    $('#ExceptionRefno').val(refno);
    $('#ExceptionContactNameHeader').text(clientName);

    $('.togglePanel').hide();

    $('#ModalAddToException').modal('show');

}



const getGroupCodeList = () => {
    $.getJSON('/groupcodelist', {}, (data) => {


        groupCodeList = data
        groupCodeListUmuttable = data

    })
}

const openAddUser = () => {


    document.getElementById('FnameInput').style.border = "1px solid #ced4da"
    document.getElementById('LnameInput').style.border = "1px solid #ced4da"

    $('#addUserModal').modal('show');

    $(groupList).empty();

    //<a href='Select group' class="dropdown-item" >Select group</div>
    let html = ``
    //$(groupList).append(html);

    groupCodeList.map((i, k) => {

        let finalVal = JSON.stringify(i)
        let html = `    
                      <a target="_blank" class="dropdown-item" onclick='return selectedGroup(${finalVal})'>${i.description}</a>
                    `
        let val = escapeHtml(html)
        $(groupList).append(unescapeHtml(val));
    })



}



const openEditUser = (val) => {

    document.getElementById('FnameInputEdit').style.border = "1px solid #ced4da"
    document.getElementById('LnameInputEdit').style.border = "1px solid #ced4da"



    viewContactClient(val)
}



const openAddUserEngagement = () => {
    //addUserEngagementModal


    $('#addUserEngagementModal').modal('show');

    searchEngagementUser();
}

const closeAddUser = () => {
    $('#addUserModal').modal('hide');

    FName = ''
    document.getElementById('FnameInput').value = ''
    MName = ''
    document.getElementById('MnameInput').value = ''
    LName = ''
    document.getElementById('LnameInput').value = ''
    Salutation = ''
    document.getElementById('salutationInput').value = ''
    Email = ''
    document.getElementById('emailInput').value = ''
    Designation = ''
    document.getElementById('designationInput').value = ''
    designated_LOS = ''
    document.getElementById('designatedLos').value = ''
    GroupCode = ''
    document.getElementById('ContactNumberInput').value = ''
    ContactNumber = ''
    document.getElementById('myInput').value = 'Select group'

    

    


    //document.getElementById('designatedLos').value = ''
    /*designatedGroup*/

}

const closeEditUser = () => {

    $('#editUserModal').modal('hide')

    FName = ''
    MName = ''
    LName = ''
    document.getElementById('FnameInputEdit').value = ''
    document.getElementById('MnameInputEdit').value = ''
    document.getElementById('LnameInputEdit').value = ''
    Salutation = ''
    document.getElementById('salutationInputEdit').value = ''
    Email = ''
    document.getElementById('emailInputEdit').value = ''
    Designation = ''
    document.getElementById('designationInputEdit').value = ''
    designated_LOS = ''
    document.getElementById('designatedLosEdit').value = ''
    GroupCode = ''
    document.getElementById('myInputEdit').value = 'Select group'


    document.getElementById('updateButton').classList.add('disableBtn')
    document.getElementById('updateButton').disabled = true



}


const deleteUser = (val, state) => {



    modalState = state



    if (state == "client") {
        selectedContactClient = val
        $('#deleteConfirmModal').modal('show')

        document.getElementById('emailToDelete').innerText = val.email
    }

    else if (state == "engagement") {

        selectedEngagementContact = val
        document.getElementById('emailToDelete').innerText = val.email

        $('#deleteConfirmModal').modal('show')
    }


}

const confirmDelete = (v) => {
    if (modalState == "client") {
        $.getJSON('/delete_client', {
            i_variable: selectedContactClient.id
        }, (data) => {

            selectedContactClient = ''

            PopulateCClist(ClientRef)
            $('#deleteConfirmModal').modal('hide')

            document.getElementById('statusContainer').classList.add('d-block');
            document.getElementById('statusContainer').classList.remove('d-none')
            document.getElementById('statusText').innerText = " User successfully removed!"

            setTimeout(() => {
                document.getElementById('statusContainer').classList.remove('d-flex')
                document.getElementById('statusContainer').classList.add('d-none')
                document.getElementById('statusText').innerText = ""
            }, 2000)

        })
    }
    else {
        $.getJSON('/delete_engagement', {
            i_variable: selectedEngagementContact.id
        }, (data) => {
            selectedEngagementContact = ''


            PopulateEClist(ClientRef)
            $('#deleteConfirmModal').modal('hide')

            document.getElementById('statusContainerUser').classList.add('d-block')
            document.getElementById('statusContainerUser').classList.remove('d-none')
            document.getElementById('statusText').innerText = " User successfully removed!"

            setTimeout(() => {
                document.getElementById('statusContainerUser').classList.remove('d-flex')
                document.getElementById('statusContainerUser').classList.add('d-none')
                document.getElementById('statusText').innerText = ""
            }, 2000)

        })
    }
}

const cancelDelete = () => {
    $('#deleteConfirmModal').modal('hide')
    selectedContactClient = ''
}

const closeAddUserEngagement = () => {

    $('#addUserEngagementModal').modal('hide');

    document.getElementById('engagementInputEmail').value = ''
    document.getElementById('engagementInputDesignation').value = ''
    document.getElementById('engagementInputGroup').value = ''
    document.getElementById('myInputEngagementUser').value = ''

    document.getElementById('addbutton').disabled = true;
    document.getElementById('addbutton').classList.add('disableBtn')
    
}

function CheckValue(e) {
    if ($(e).val() == 'True') {

        if ($('#ClientP').is(":visible")) {
            $('#ClientP').fadeOut();
        }

        $('.EngagementP').removeClass('d-none');

        $('.EngagementP').fadeIn();

        $("#FirstName").val('');
        $("#MiddleName").val('');
        $("#LastName").val('');

        $("#Name").rules("add", "required");
        $("#Designation").rules("add", "required");


        $("#Name").prop('disabled', false);
        //$("#Designation").prop('disabled', false);

    } else {

        if ($('.EngagementP').is(":visible")) {
            $('.EngagementP').fadeOut();
        }

        $('#ClientP').removeClass('d-none');

        $('#ClientP').fadeIn();

        $("#Name").val('');

        $("#Name").rules("remove", "required");
        $("#Designation").rules("remove", "required");
        $('#Name-error').hide();
        $('#Designation-error').hide();


        $("#Name").prop('disabled', true);
        //$("#Designation").prop('disabled', true);
    }

}

function DisableAddContacts() {
    if ($('#tblmycContacts tbody tr').length >= myClientContactLimit) {
        $('#BtnAddContact').prop('disabled', true);
    } else {
        $('#BtnAddContact').prop('disabled', false);
    }
}







//selectedClient

const viewContactClient = (val) => {


    $('#editUserModal').modal('show');



    $.getJSON('/edit_client', {
        i_variable: val
    }, (v) => {
        /*    document.getElementById('designatedGroup').innerText = val.description*/

        if (v.length != 0) {

            validEmail = true;

            FName = v[0].first_Name
            MName = v[0].middle_Name
            LName = v[0].last_Name
            Salutation = v[0].salutation
            Email = v[0].email
            Designation = v[0].designation
            GroupCode = v[0].designated_Group_Type
            LOS = v[0].designated_LOS
            ContactNumber = v[0].contact_Number
            CCode = val
            GroupType = v[0].designated_Group_Id
            document.getElementById('FnameInputEdit').value = FName
            document.getElementById('MnameInputEdit').value = MName
            document.getElementById('LnameInputEdit').value = LName
            document.getElementById('ContactNumberInputEdit').value = ContactNumber
            let filteredGroup = ''

            groupCodeList.map((i, k) => {
                // v[0] is a DataProtection token (opaque), not a group code.
                // The group selection is handled by the dropdown UI, not by matching tokens.
            })


            //filter group list and equal it to input
            document.getElementById('myInputEdit').value = filteredGroup


            document.getElementById('myInput')
            document.getElementById('designatedLosEdit').value = LOS



            document.getElementById('salutationInputEdit').value = Salutation
            document.getElementById('emailInputEdit').value = Email
            document.getElementById('designationInputEdit').value = Designation


            //groupListEdit

            $(groupList).empty();
            $(groupListEdit).empty();




            groupCodeList.map((i, k) => {

                let finalVal = JSON.stringify(i)
                let html = `    
                       <a target="_blank" class="dropdown-item" onclick='return selectedGroupEdit(${finalVal})'>${i.description}</a>
                    `
                let val = escapeHtml(html)
                $(groupListEdit).append(unescapeHtml(val));
            })

        }
    })
}

//addingForm editingForm js

const FirstNameUpdate = () => {

    let nameInput = document.getElementById('FnameInput').value


    if (nameInput != "") {
        document.getElementById('FnameInput').style.border = "1px solid #ced4da"
    }
    else {
        document.getElementById('FnameInput').style.border = "1px solid red"
    }
    FName = nameInput;
}

const MiddleNameUpdate = () => {

    let nameInput = document.getElementById('MnameInput').value

    MName = nameInput;
}

const LastNameUpdate = () => {

    let nameInput = document.getElementById('LnameInput').value


    if (nameInput != "") {
        document.getElementById('LnameInput').style.border = "1px solid #ced4da"
    }
    else {
        document.getElementById('LnameInput').style.border = "1px solid red"
    }

    LName = nameInput;
}

const ContactNumberUpdate = () => {
    let nameInput = document.getElementById('ContactNumberInput').value

    

    if (checkIfValidContact.test(nameInput)) {
        ContactNumber = nameInput;
    }
    else {
        ContactNumber = ''
        document.getElementById('ContactNumberInput').value = ''

    }
}

const salutationUpdate = () => {

    let salutationInput = document.getElementById('salutationInput').value

    Salutation = salutationInput;
}

const emailUpdate = () => {

    let emailInput = document.getElementById('emailInput').value

    Email = emailInput;

    if (validateEmail(emailInput) == null) {
        document.getElementById('emailInput').style.border = "1px solid red"
        document.getElementById('emailInvalidLabel').classList.remove('d-none')
        document.getElementById('invalidIcon').classList.remove('d-none')
        validEmail = false
    }
    else {
        document.getElementById('emailInput').style.border = "1px solid #ced4da"
        document.getElementById('emailInvalidLabel').classList.add('d-none')
        document.getElementById('invalidIcon').classList.add('d-none')
        validEmail = true
    }
}

const designationUpdate = () => {

    let designationInput = document.getElementById('designationInput').value


    Designation = designationInput;
}

const selectedGroup = (val) => {

    

    document.getElementById('groupList').style.display = "none"

    //GroupType new previous group_Code_WO_Desc in payload
    $.getJSON('/losbygroupcodelist', {
        groupcode: val.group_Type,
        Group_Id: val.group_Id,
        GroupType :val.group_Code_WO_Desc
    }, (v) => {

        
        //document.getElementById('designatedGroup').innerText = val.description
        document.getElementById('myInput').value = val.description
        //document.getElementById('designatedGroupEdit').innerText = val.description


        if (v.length != 0) {
            document.getElementById('designatedLos').value = v[0].los
            //document.getElementById('designatedLosEdit').value = v[0].los

            document.getElementById('groupContainer').style.border = "1px solid #ced4da"
            Group = val.description
            GroupCode = val.group_Id
            GroupType = val.group_Code_WO_Desc
            LOS = v[0].los
        }
    })

    return false
}

const selectedGroupEdit = (val) => {

    document.getElementById('groupListEdit').style.display = "none"

    $.getJSON('/losbygroupcodelist', {
        groupcode: val.group_Type,
        Group_Id: val.group_Id,
        GroupType: val.group_Code_WO_Desc
    }, (v) => {

        //document.getElementById('designatedGroup').innerText = val.description
        document.getElementById('myInputEdit').value = val.description


        if (v.length != 0) {

            document.getElementById('designatedLos').value = v[0].los
            document.getElementById('designatedLosEdit').value = v[0].los

            document.getElementById('groupContainer').style.border = "1px solid #ced4da"
            Group = val.description
            GroupCode = val.group_Id
            GroupType = val.group_Code_WO_Desc
            LOS = v[0].los
        }
    })


    document.getElementById('updateButton').classList.remove('disableBtn')
    document.getElementById('updateButton').disabled = false

    return false
}


//Edit form contact client

const FirstNameUpdateEdit = () => {

    let nameInput = document.getElementById('FnameInputEdit').value



    if (nameInput != "") {
        document.getElementById('FnameInputEdit').style.border = "1px solid #ced4da"
    }
    else {
        document.getElementById('FnameInputEdit').style.border = "1px solid red"
    }
    FName = nameInput;
}

const MiddleNameUpdateEdit = () => {

    let nameInput = document.getElementById('MnameInputEdit').value

    MName = nameInput;
}

const LastNameUpdateEdit = () => {

    let nameInput = document.getElementById('LnameInputEdit').value


    if (nameInput != "") {
        document.getElementById('LnameInputEdit').style.border = "1px solid #ced4da"
    }
    else {
        document.getElementById('LnameInpuEdit').style.border = "1px solid red"
    }

    LName = nameInput;
}

const ContactNumberUpdateEdit = () => {
    let nameInput = document.getElementById('ContactNumberInputEdit').value

    
    
    if (checkIfValidContact.test(nameInput)) {
        ContactNumber = nameInput;
    }
    else {
        ContactNumber = ''
        document.getElementById('ContactNumberInputEdit').value = ''

    }


    document.getElementById('updateButton').classList.remove('disableBtn')
    document.getElementById('updateButton').disabled = false

}

const salutationUpdateEdit = () => {

    let salutationInput = document.getElementById('salutationInputEdit').value

    Salutation = salutationInput;

    document.getElementById('updateButton').classList.remove('disableBtn')
    document.getElementById('updateButton').disabled = false
}

const emailUpdateEdit = () => {

    let emailInput = document.getElementById('emailInputEdit').value

    Email = emailInput;


    if (validateEmail(emailInput) == null) {
        document.getElementById('emailInputEdit').style.border = "1px solid red"
        document.getElementById('emailInvalidLabelEdit').classList.remove('d-none')
        document.getElementById('invalidIconEdit').classList.remove('d-none')
        validEmail = false
    }
    else {
        document.getElementById('emailInputEdit').style.border = "1px solid #ced4da"
        document.getElementById('emailInvalidLabelEdit').classList.add('d-none')
        document.getElementById('invalidIconEdit').classList.add('d-none')
        validEmail = true
    }

    document.getElementById('updateButton').classList.remove('disableBtn')
    document.getElementById('updateButton').disabled = false
}

const designationUpdateEdit = () => {

    let designationInput = document.getElementById('designationInputEdit').value

    Designation = designationInput;

    document.getElementById('updateButton').classList.remove('disableBtn')
    document.getElementById('updateButton').disabled = false
}


const updateClientUser = () => {


    if (validEmail && FName != "" && LName != "") {
        $.getJSON('/updateuserclient', {
            first_name: FName,
            middle_name: "",
            last_name: LName,
            contact_number: ContactNumber,
            salutation: Salutation,
            email: Email,
            designation: Designation,
            //designated_Group_Type: GroupCode,
            designated_LOS: LOS,
            searched_Employee_Code: null,
            client_Code: ClientCode,
            Designated_Group_Id: GroupCode,
            Id: CCode,
            Group_Code_WO_Desc: GroupType
        }, (res) => {

            PopulateCClist(ClientRef)

            validEmail = false

            Name = ''
            FName = ''
            MName = ''
            LName = ''
            ContactNumber = ''
            Salutation = ''
            Email = ''
            Designation = ''
            Group = ''
            LOS = ''
            GroupCode = ''

            document.getElementById('designationInputEdit').value = ''
            document.getElementById('designatedLos').value = ''

            document.getElementById('updateButton').classList.add('disableBtn')
            document.getElementById('updateButton').disabled = true

            /* ClientCode = ''*/
            document.getElementById('statusContainerUser').classList.add('d-block')
            document.getElementById('statusContainerUser').classList.remove('d-none')
            document.getElementById('statusText').innerText = " User successfully updated!"

            setTimeout(() => {
                document.getElementById('statusContainerUser').classList.remove('d-flex')
                document.getElementById('statusContainerUser').classList.add('d-none')
                document.getElementById('statusText').innerText = ""
            }, 2000)



            $('#editUserModal').modal('hide');
        })

            .fail((err) => {

                $('#editUserModal').modal('hide');
                validEmail = false

                Name = ''
                FName = ''
                MName = ''
                LName = ''
                ContactNumber = ''
                Salutation = ''
                Email = ''
                Designation = ''
                Group = ''
                LOS = ''
                GroupCode = ''

                document.getElementById('updateButton').classList.add('disableBtn')
                document.getElementById('updateButton').disabled = true

                document.getElementById('FnameInputEdit').value = ''
                document.getElementById('MnameInputEdit').value = ''
                document.getElementById('LnameInputEdit').value = ''
                document.getElementById('salutationInputEdit').value = ''
                document.getElementById('emailInputEdit').value = ''
                document.getElementById('designationInputEdit').value = ''
                document.getElementById('myInputEdit').value = ''
                document.getElementById('designatedLosEdit').value = ''
                document.getElementById('ContactNumberInputEdit').value = ''


                document.getElementById('statusContainerUser').classList.add('d-block')
                document.getElementById('statusIcon').classList.remove('icon-circle-checkmark-fill')
                document.getElementById('statusIcon').classList.add('icon-circle-delete-fill')

                document.getElementById('statusContainerUser').style.backgroundColor = "red"
                document.getElementById('statusContainerUser').classList.remove('d-none')
                document.getElementById('statusText').innerText = "Something went wrong!"

                setTimeout(() => {
                    document.getElementById('statusIcon').classList.add('icon-circle-checkmark-fill')
                    document.getElementById('statusIcon').classList.remove('icon-circle-delete-fill')

                    document.getElementById('statusContainerUser').classList.remove('d-flex')
                    document.getElementById('statusContainerUser').style.backgroundColor = "green"
                    document.getElementById('statusContainerUser').classList.add('d-none')
                    document.getElementById('statusText').innerText = ""


                }, 2000)
            })

    }
    if (!validEmail) {
        document.getElementById('emailInputEdit').style.border = "1px solid red"
        document.getElementById('emailInvalidLabelEdit').classList.remove('d-none')
        document.getElementById('invalidIconEdit').classList.remove('d-none')
    }

    if (FName != "") {

    }
    else {
        document.getElementById('FnameInputEdit').style.border = "1px solid red"
    }

    if (LName != "") {

    }
    else {
        document.getElementById('LnameInputEdit').style.border = "1px solid red"
    }


    if (GroupCode != "") {

    }
    else {
        document.getElementById('groupContainer').style.border = "1px solid red"
    }

}

const addClientUser = () => {



    if (validEmail && FName != "" && LName != "" && GroupCode != "") {
        $.getJSON('/adduserclient', {
            first_name: FName,
            middle_name: "",
            last_name: LName,
            contact_number: ContactNumber,
            salutation: Salutation,
            email: Email,
            designation: Designation,
            //designated_Group_Type: GroupCode,
            designated_LOS: LOS,
            searched_Employee_Code: null,
            client_Code: ClientCode,
            Designated_Group_Id: GroupCode,
            Group_Code_WO_Desc: GroupType
        }, (res) => {



            PopulateCClist(ClientRef)

            validEmail = false

            Name = ''
            FName = ''
            MName = ''
            LName = ''
            ContactNumber = ''
            Salutation = ''
            Email = ''
            Designation = ''
            Group = ''
            LOS = ''
            GroupCode = ''

            document.getElementById('FnameInput').value = ''
            document.getElementById('MnameInput').value = ''
            document.getElementById('LnameInput').value = ''
            document.getElementById('salutationInput').value = ''
            document.getElementById('emailInput').value = ''
            document.getElementById('designationInput').value = ''
            document.getElementById('myInput').value = ''
            document.getElementById('designatedLos').value = ''
            document.getElementById('ContactNumberInput').value = ''
            /* ClientCode = ''*/

            //statusModal

            document.getElementById('statusContainerUser').classList.add('d-block')
            document.getElementById('statusContainerUser').classList.remove('d-none')
            document.getElementById('statusText').innerText = " User successfully added!"

            setTimeout(() => {
                document.getElementById('statusContainerUser').classList.remove('d-flex')
                document.getElementById('statusContainerUser').classList.add('d-none')
                document.getElementById('statusText').innerText = ""
            }, 2000)


            $('#addUserModal').modal('hide');
        })

            .fail((err) => {

                $('#addUserModal').modal('hide');
                validEmail = false

                Name = ''
                FName = ''
                MName = ''
                LName = ''
                ContactNumber = ''
                Salutation = ''
                Email = ''
                Designation = ''
                Group = ''
                LOS = ''
                GroupCode = ''

                document.getElementById('FnameInput').value = ''
                document.getElementById('MnameInput').value = ''
                document.getElementById('LnameInput').value = ''
                document.getElementById('salutationInput').value = ''
                document.getElementById('emailInput').value = ''
                document.getElementById('designationInput').value = ''
                document.getElementById('myInput').value = ''
                document.getElementById('designatedLos').value = ''
                document.getElementById('ContactNumberInput').value = ''


                document.getElementById('statusContainerUser').classList.add('d-block')
                document.getElementById('statusIcon').classList.remove('icon-circle-checkmark-fill')
                document.getElementById('statusIcon').classList.add('icon-circle-delete-fill')

                document.getElementById('statusContainerUser').style.backgroundColor = "red"
                document.getElementById('statusContainerUser').classList.remove('d-none')
                document.getElementById('statusText').innerText = "Something went wrong!"

                setTimeout(() => {
                    document.getElementById('statusIcon').classList.add('icon-circle-checkmark-fill')
                    document.getElementById('statusIcon').classList.remove('icon-circle-delete-fill')

                    document.getElementById('statusContainerUser').classList.remove('d-flex')
                    document.getElementById('statusContainerUser').style.backgroundColor = "green"
                    document.getElementById('statusContainerUser').classList.add('d-none')
                    document.getElementById('statusText').innerText = ""


                }, 2000)
            })
    }
    if (!validEmail) {
        document.getElementById('emailInput').style.border = "1px solid red"
        document.getElementById('emailInvalidLabel').classList.remove('d-none')
        document.getElementById('invalidIcon').classList.remove('d-none')
    }

    if (FName != "") {

    }
    else {
        document.getElementById('FnameInput').style.border = "1px solid red"
    }

    if (LName != "") {

    }
    else {
        document.getElementById('LnameInput').style.border = "1px solid red"
    }


    if (GroupCode != "") {

    }
    else {
        document.getElementById('groupContainer').style.border = "1px solid red"
    }
}




const focusInput = (v) => {
    document.getElementById(v).style.display = 'block';
}

const removeFocusInput = (v) => {


    //so click will work before hiding
    setTimeout(() => {
        document.getElementById(v).style.display = 'none';
    }, 500)
}

function filterFunction(inputId, dropdownId) {
    var input, filter, ul, li, a, i;
    input = document.getElementById(inputId);
    filter = input.value.toUpperCase();
    div = document.getElementById(dropdownId);
    a = div.getElementsByTagName("a");
    for (i = 0; i < a.length; i++) {
        txtValue = a[i].textContent || a[i].innerText;
        if (txtValue.toUpperCase().indexOf(filter) > -1) {
            a[i].style.display = "";
        } else {
            a[i].style.display = "none";
        }
    }
}


const selectedSearchUser = (v) => {


    const guid = v.id

    document.getElementById('engagementuserList').style.display = 'none';

    document.getElementById('addbutton').disabled = false;
    document.getElementById('addbutton').classList.remove('disableBtn')

    $.getJSON('/view_user_engagement', {
        employee_code_variable: v.id
    }, (response) => {


        if(response.length != 0){
            let finalRes = response[0]

            document.getElementById('engagementInputEmail').value = finalRes.email
            document.getElementById('engagementInputDesignation').value = finalRes.designation
            document.getElementById('engagementInputGroup').value = finalRes.group
            document.getElementById('myInputEngagementUser').value = finalRes.name
    
            emailEngagement = finalRes.email
            designationEngagement = finalRes.designation
            groupEngagement = finalRes.group
            EName = finalRes.name
            groupId = finalRes.group_Id
        }
        else{

        }

    })

}

const addEmailEngagement = (v) => {

    emailEngagement = document.getElementById('engagementInputEmail').value

    
}

const editEmailEngagement = (v) => {

    //change while typing


    emailEngagement = document.getElementById('engagementInputEmailEdit').value


}

const openEditEngagementUser = (id) => {

    $('#editUserEngagementModal').modal('show');

    $.getJSON('/edit_engagement', {
        i_variable: id
    }, (response) => {


        let finalRes = response[0]

        document.getElementById('engagementInputEmailEdit').value = finalRes.email
        document.getElementById('engagementInputDesignationEdit').value = finalRes.designation
        document.getElementById('engagementInputGroupEdit').value = finalRes.designated_Group
        document.getElementById('engagementInputNameEdit').value = finalRes.name

        emailEngagement = finalRes.email
        designationEngagement = finalRes.designation
        groupEngagement = finalRes.designated_Group
        EName = finalRes.name
        EEcode = finalRes.id
    })
}



const searchEngagementUser = (v) => {
    //stafflist


    $.getJSON('/stafflist_for_engagement_users', {
        group_code_variable: groupcode,
        client_code_variable: ClientRef
    }, (v) => {


        $(engagementuserList).empty();

        //<a href='Select group' class="dropdown-item" >Select group</div>
        let html = ``
        //$(groupList).append(html);

        v.map((i, k) => {

            let finalVal = JSON.stringify(i)
            let html = `    
                      <a target="_blank" class="dropdown-item" onclick='return selectedSearchUser(${finalVal})'>${i.name}</a>
                    `
            let val = escapeHtml(html)
            $(engagementuserList).append(unescapeHtml(val));
        })
    })
}

const addUserEngagement = () => {

    //group_Id is still null from api temporary only to 0
    
    
    $.getJSON('/adduserengagement', {
        Designation: designationEngagement,
        Email: emailEngagement,
        Client_Code: ClientCode,
        Name: EName,
        // Designated_Group: groupId,
        Designated_Group: groupcode
    }, (response) => {



        document.getElementById('engagementInputEmail').value = ''
        document.getElementById('engagementInputDesignation').value = ''
        document.getElementById('engagementInputGroup').value = ''
        document.getElementById('myInputEngagementUser').value = ''

        document.getElementById('statusContainerUser').classList.add('d-block')
        document.getElementById('statusContainerUser').classList.remove('d-none')
        document.getElementById('statusText').innerText = " User successfully addedd!"

        setTimeout(() => {
            document.getElementById('statusContainerUser').classList.remove('d-flex')
            document.getElementById('statusContainerUser').classList.add('d-none')
            document.getElementById('statusText').innerText = ""
        }, 2000)

        emailEngagement = ''
        designationEngagement = ''
        groupEngagement = ''
        EName = ''
        groupId = ''
        PopulateEClist(ClientRef);
        closeAddUserEngagement()

    })
}



const updateUserEngagement = () => {




    $.getJSON('/updateuserengagement', {
        Id: EEcode,
        Email: emailEngagement,
    }, (response) => {



        document.getElementById('engagementInputEmail').value = ''
        document.getElementById('engagementInputDesignation').value = ''
        document.getElementById('engagementInputGroup').value = ''
        document.getElementById('myInputEngagementUser').value = ''

        document.getElementById('statusContainerUser').classList.add('d-block')
        document.getElementById('statusContainerUser').classList.remove('d-none')
        document.getElementById('statusText').innerText = " User successfully updated!"

        setTimeout(() => {
            document.getElementById('statusContainerUser').classList.remove('d-flex')
            document.getElementById('statusContainerUser').classList.add('d-none')
            document.getElementById('statusText').innerText = ""
        }, 2000)

        emailEngagement = ''
        designationEngagement = ''
        groupEngagement = ''
        EName = ''
        EEcode = ''
        PopulateEClist(ClientRef);
        closeAddUserEngagement()

        $('#editUserEngagementModal').modal('hide');



    })
}




const confirmDeleteEngagement = (val) => {

}

const ShowExceptionInfo = (e) => {


    



    $.getJSON('/view_exception', {
        i_variable: e.id
    }, (response) => {



        if (response.length != 0) {

            document.getElementById('viewCName').innerText = response[0].clientName
            document.getElementById('viewCC').value = response[0].clientCode
            document.getElementById('viewReason').value = response[0].reason
            document.getElementById('viewOtherReason').value = response[0].other_Reason

            if (response[0].reason == "Others") {
                document.getElementById('otherReasonContainer').classList.remove('d-none')


            }

            else {
                document.getElementById('otherReasonContainer').classList.add('d-none')
            }
        }

        $('#ModalShowException').modal('show');

    })


}


const closeViewException = () => {
    document.getElementById('otherReasonContainer').classList.add('d-none')


    $('#ModalShowException').modal('hide');
}


//Viewing of exception
const changeReason = (target, finalVal, index) => {    
    
    target.stopPropagation();
    
    $(reasonList).empty();
    $(reasonValue).empty()
    $(reasonValue).append(`<span>Select reason</span>`)
    let id = "tooltip" + finalVal.clientName + index
    let onCLickElement = document.getElementById(id);
    let onClickElementByClass = document.getElementsByClassName('my-tooltip')
    document.getElementById('other_update').classList.add('d-none')


    exceptionID = finalVal.id
    selectedClient = finalVal.clientName
    ClientCode = finalVal.clientCode
    other_reason = finalVal.other_reason
    reasonVal = finalVal.reason
    invoice_Number = finalVal.reference_No
    bill_Number = finalVal.bill_No

    //for persist
    P_reasonVal = finalVal.reason
    P_selectedClient = finalVal.clientName

    if (finalVal.reason != "Others") {
        other_enable = false

        document.getElementById('other_update').classList.add('d-none')
    }
    else {
        other_enable = true

        document.getElementById('other_update').classList.remove('d-none')
    }

    onCLickElement.style.display = "none";

    $.getJSON('/view_exception', {
        i_variable: exceptionID
    }, (response) => {
        $('#ModalUpdateReason').modal('show');
        let other_values = response[0].other_Reason

        //eto ang bug fixes
        if(response[0].reason == "Others"){
            document.getElementById('currentReasonLabel').innerText = other_values
            reasonVal = response[0].previous_Reason

                  //must removed excess Others
        document.getElementById('otherReasonUpdateInput').value = other_values.replace(/Others - /ig, '')

        }
        else{
          
            document.getElementById('currentReasonLabel').innerText = response[0].reason
            document.getElementById('otherReasonUpdateInput').value = ""
            reasonVal = response[0].reason
            
        }
        

    
        // Other_Reason = response[0].other_Reason

  
        P_other_reason = response[0].other_Reason

        document.getElementById('totalCounterUpdate').innerText = other_values.length
        $.getJSON('/load_reason', {}, (data) => {
            data.map((item, k) => {

                let initialVal = item
                initialVal['description'] = item.description.replace(/'/ig, "’")


                let finalVal = JSON.stringify(initialVal)



                let html = `    
                    <li><div class="dropdown-item" onclick='selectedReason(${finalVal}, "update")'>${item.description}</div></li>
                    `

                let val = escapeHtml(html)
                $(reasonList).append(unescapeHtml(val));
            })

        })


    })



}


const updateException = () => {



    if (updatedReason == "" && other_reason == P_other_reason) {


        document.getElementById('updatedReasonContainer').style.border = "1px solid red"
    }

    else if (other_enable && updatedOtherReason == "") {
        document.getElementById('otherReasonUpdateInput').style.border = "1px solid red"
    }

    else {
        document.getElementById('updatedReasonContainer').style.border = '1px solid #E0E1E1'



        $.getJSON('/update_exception', {
            Id: exceptionID,
            ClientCode: ClientCode,
            ClientName: selectedClient,
            Reason: updatedReason,
            Other_Reason: updatedOtherReason,
            User_Name: document.getElementById('username').innerText,
            Action: "Update reason of exception",
            Previous_Reason: reasonVal,
            Invoice_Number: invoice_Number,
            bill_No: bill_Number

        }, (response) => {

            document.getElementById('statusContainer').classList.add('d-block');
            document.getElementById('statusContainer').classList.remove('d-none')

            //$(statusIcon).append(`<span> Successfully update exception reason.</span>`)

            // NEW
            $('#message_notif').text('Successfully update exception reason.');

            setTimeout(() => {
                document.getElementById('statusContainer').classList.remove('d-flex')
                document.getElementById('statusContainer').classList.add('d-none')
                $(statusIcon).empty()

            }, 3000)

            $(reasonValue).empty()
            reasonVal = ''
            selectedClient = ''
            other_reason = ''

            document.getElementById('SubmitExceptionUpdate').disabled = true;
            document.getElementById('SubmitExceptionUpdate').classList.add('disableBtn');

            $('#ModalUpdateReason').modal('hide');
            loadException()


        })
            .fail((err) => {
                document.getElementById('statusContainer').classList.add('d-block');
                document.getElementById('statusContainer').classList.remove('d-none')
                //$(statusIcon).append(`<span>Error updating exception reason.</span>`)

                // NEW 
                $('#message_notif').text('Error updating exception reason.');

                document.getElementById('SubmitExceptionUpdate').disabled = true;
                document.getElementById('SubmitExceptionUpdate').classList.add('disableBtn');

                setTimeout(() => {
                    document.getElementById('statusContainer').classList.remove('d-flex')
                    document.getElementById('statusContainer').classList.add('d-none')
                    $(statusIcon).empty()

                }, 3000)
            })
    }



}

const cancelMove = () => {

    reasonVal = ''
    other_reason = ''
    updatedOtherReason = ''
    updatedReason = ''

    document.getElementById('otherReasonAddInput').value = ''
    document.getElementById('other_add').classList.add('d-none')
    $(reasonValue).empty()
    $(reasonValue).append('<span>Select reason</span>')
    $('#ModalAddToExceptionConfirmation').modal('hide');
}



const cancelUpdateException = () => {

    reasonVal = ''
    other_reason = ''
    updatedOtherReason = ''
    updatedReason = ''

    document.getElementById('otherReasonUpdateInput').value = ''
    document.getElementById('other_update').classList.add('d-none')
    $(reasonValue).empty()
    $(reasonValue).append('<span>Select reason</span>')
    $('#ModalUpdateReason').modal('hide');
}

const cancelException = () => {

    reasonVal = ''
    other_reason = ''
    updatedOtherReason = ''
    updatedReason = ''

    document.getElementById('otherReasonDeleteInput').value = ''
    document.getElementById('other_delete').classList.add('d-none')
    $(removereasonValue).empty()
    $(removereasonValue).append('<span>Select reason</span>')
    $('#ModalRemoveFromException').modal('hide');
}


const moveToException = (value) => {


    
    ClientCode = value.clientId
    selectedClient = value.clientName
    bill_Number = value.billNo
    invoice_Number = value.reference_No
    GroupCode = value.group_Code;

    $('#ModalAddToExceptionConfirmation').modal('show');


    $(reasonList).empty();
    $(reasonValue).empty()
    $(reasonValue).append(`<span>Select reason</span>`)

    document.getElementById('other_add').classList.add('d-none')

    $.getJSON('/load_reason', {}, (data) => {
        console.log("response in load reason", data)
        data.map((item, k) => {

            let initialVal = item
            initialVal['description'] = item.description.replace(/'/ig, "’")


            let finalVal = JSON.stringify(initialVal)




            let html = `    
                    <li><div class="dropdown-item" onclick='selectedReason(${finalVal}, "add")'>${item.description}</div></li>
                    `
            let val = escapeHtml(html)
            $(reasonList).append(unescapeHtml(val));
        })

    })
}

const selectedReason = (v, label) => {

    

    document.getElementById('SubmitException').disabled = false;
    document.getElementById('SubmitException').classList.remove('disableBtn');

    

    if(label == "update" && v.description != reasonVal){
        

        
    document.getElementById('SubmitExceptionUpdate').disabled = false;
    document.getElementById('SubmitExceptionUpdate').classList.remove('disableBtn');

    }

    else if(label == "update" && v.description == reasonVal){
        document.getElementById('SubmitExceptionUpdate').disabled = true;
        document.getElementById('SubmitExceptionUpdate').classList.add('disableBtn');
    }

    if (label != "update") {
        reasonVal = v.description
    }
    else{
        
    
    }

        

   
    updatedReason = v.description

    $(reasonValue).empty()

    document.getElementById('updatedReasonContainer').style.border = '1px solid #E0E1E1'
    document.getElementById('addReasonContainer').style.border = '1px solid #E0E1E1'

    if (v.description == "Others") {
        other_enable = true


        document.getElementById('other_add').classList.remove('d-none')
        document.getElementById('other_update').classList.remove('d-none')

        if(label != "update"){
            reasonVal = v.description
        }
        else{
            // reasonVal = v.description + " - " + reasonVal
        }
    }
    else {
        other_enable = false

        document.getElementById('other_add').classList.add('d-none')
        document.getElementById('other_update').classList.add('d-none')
    }

    let html = `<span>${v.description}</span>`
    $(reasonValue).append(html)

}


var x = document.getElementById('otherReasonAddInput');
x.addEventListener('input', function () {
    a = x.value
    document.getElementById('totalCounterAdd').innerText = a.length
});

const otherReasonInputAdd = (v) => {
    let a = document.getElementById('otherReasonAddInput').value

    other_reason = a


    if (a != "") {
        document.getElementById('otherReasonAddInput').style.border = '1px solid #E0E1E1'

    }
    else {
        document.getElementById('otherReasonAddInput').style.border = '1px solid red'
    }
}


var y = document.getElementById('otherReasonUpdateInput');
y.addEventListener('input', function () {
    a = y.value
    document.getElementById('totalCounterUpdate').innerText = a.length
});

const otherReasonInputUpdate = (v) => {
    let a = document.getElementById('otherReasonUpdateInput').value

    updatedOtherReason = a

   
    if (a != "") {
        document.getElementById('otherReasonUpdateInput').style.border = '1px solid #E0E1E1'

        if(a == P_other_reason){
            

            document.getElementById('SubmitExceptionUpdate').disabled = true;
            document.getElementById('SubmitExceptionUpdate').classList.add('disableBtn');

          
        }
        else if(a != P_other_reason){
            document.getElementById('SubmitExceptionUpdate').disabled = false;
            document.getElementById('SubmitExceptionUpdate').classList.remove('disableBtn');
        }

    }
    else {
        document.getElementById('otherReasonUpdateInput').style.border = '1px solid red'
    }
}


var z = document.getElementById('otherReasonDeleteInput');
z.addEventListener('input', function () {
    a = z.value
    document.getElementById('totalCounterDelete').innerText = a.length
});
const otherReasonInputDelete = (v) => {
    let a = document.getElementById('otherReasonDeleteInput').value

    updatedOtherReason = a


    if (a != "") {
        document.getElementById('otherReasonDeleteInput').style.border = '1px solid #E0E1E1'

    }
    else {
        document.getElementById('otherReasonDeleteInput').style.border = '1px solid red'
    }
}

const selectedReasonRemove = (v) => {


    updatedReason = v
    $(removereasonValue).empty()

    document.getElementById('removereasonValue').style.border = '1px solid #E0E1E1'

    if (v == "Others") {
        other_enable = true


        document.getElementById('other_delete').classList.remove('d-none')
    }
    else {
        other_enable = false

        document.getElementById('other_delete').classList.add('d-none')
    }

    let html = `<span>${v}</span>`
    $(removereasonValue).append(html)

}

const removeFromException = (target, finalVal, index) => {



    target.stopPropagation();

    exceptionID = finalVal.id
    selectedClient = finalVal.clientName
    ClientCode = finalVal.clientCode
    invoice_Number = finalVal.reference_No
    bill_Number = finalVal.bill_No

    $(removereasonValue).empty()
    $(reasonListDelete).empty()
    $(removereasonValue).append('<span>Select reason</span>')

    let id = "tooltip" + finalVal.clientName + index


    let onCLickElement = document.getElementById(id);
    let onClickElementByClass = document.getElementsByClassName('my-tooltip')





    onCLickElement.style.display = "none";



    $.getJSON('/load_deletion_reason', {
    }, (response) => {



        response.map((item, k) => {
            let initialVal = item
            initialVal['description'] = item.description.replace(/'/ig, "’")


            let finalVal = JSON.stringify(initialVal)




            let html = `    
                    <li><div class="dropdown-item" onclick='selectedReasonRemove("${item.description}")'>${item.description}</div></li>
                    `
                    let val = escapeHtml(html)
            $(reasonListDelete).append(unescapeHtml(val));
        })



        $('#ModalRemoveFromException').modal('show');
    })



}

const confirmRemoveFromException = () => {

    let username = document.getElementById('username').innerText

    

    if (updatedReason == "") {
        document.getElementById('removereasonValue').style.border = "1px solid red"
    }
    else {
        document.getElementById('removereasonValue').style.border = '1px solid #E0E1E1'

        $.getJSON('/delete_exception', {
            Id: exceptionID,
            Reason: updatedReason,
            ClientCode: ClientCode,
            Other_Reason: updatedOtherReason,
            User_Responsible_Deletion: username,
            User_Name: username,
            ClientName: selectedClient,
            Invoice_number: invoice_Number,
            Action: 'Remove exception',
            bill_No: bill_Number
    
        }, (response) => {
    
            reasonVal = ''
            selectedClient = ''
            other_reason = ''
            ClientCode = ''
            updatedOtherReason = ''
            updatedReason = ''
            other_enable = false
    
    
    
            document.getElementById('other_delete').classList.add('d-none')
            document.getElementById('other_delete').classList.remove('d-flex')
    
            document.getElementById('otherReasonDeleteInput').value = ''
    
    
            document.getElementById('statusContainer').classList.add('d-block');
            document.getElementById('statusContainer').classList.remove('d-none')

            //$(statusIcon).append(`<span> Successfully removed from exception.</span>`)

            // NEW
            $('#message_notif').text('Successfully removed from exception.');
    
            setTimeout(() => {
                document.getElementById('statusContainer').classList.remove('d-flex')
                document.getElementById('statusContainer').classList.add('d-none')
                document.getElementById('statusText').innerText = ""
                $(statusIcon).empty();
            }, 3000)
    
    
            $('#ModalRemoveFromException').modal('hide');
    
            loadException()
    
        })
    }

    

}



const confirmMove = () => {



    if (reasonVal == "") {
        document.getElementById('addReasonContainer').style.border = '1px solid red'
    }

    else if (other_enable && other_reason == "") {
        document.getElementById('otherReasonAddInput').style.border = '1px solid red'
    }

    else {
        document.getElementById('updatedReasonContainer').style.border = '1px solid #E0E1E1'
        // reference_No
        $.getJSON('/add_exception', {
            ClientCode: ClientCode,
            ClientName: selectedClient,
            Reason: reasonVal,
            Other_Reason: other_reason,
            bill_No : bill_Number,
            invoice_Number: invoice_Number,
            User_Name: document.getElementById('username').innerText,
            Action: "Moved to exception",
            Group_Code: GroupCode
        }, (response) => {



            reasonVal = ''
            selectedClient = ''
            invoice_Number = ''
            other_reason = ''
            other_enable = false
            document.getElementById('otherReasonAddInput').value = ''
            $(reasonValue).empty()
            document.getElementById('other_add').classList.add('d-none')

            $(reasonValue).append(`<span>Select reason</span>`)

            //document.getElementById('statusContainer').classList.add('d-block');

            // NEW
            document.getElementById('statusContainer').classList.add('d-block');

            document.getElementById('statusContainer').classList.remove('d-none')



            //$(statusIcon).append(`Successfully move to exception.`);

            // NEW
            $('#message_notif').text('Successfully move to exception.');


            //$('#statusContainer').append(`<span id="statusIcon" class="Appkit4-icon icon-circle-checkmark-fill a-text-white">Successfully move to exception.</span>`)

            setTimeout(() => {
                document.getElementById('statusContainer').classList.remove('d-flex')
                document.getElementById('statusContainer').classList.add('d-none')
                $(statusIcon).empty()

            }, 3000)

            $('#ModalAddToExceptionConfirmation').modal('hide');

            const myparamC = pageNumber + "|" + pageSize + "|" + clientName + "|" + clientContact + "|" + clientEmail + "|" + partner + "|" + clientContactSort + "|" + clientEmailSort + "|" + partnerSort + "|" + groupSearch + "|" + groupSearchSort;
            loadClientList('/Clients/GetClients', myparamC, '#tblBodyClients', '#tblFooterClients', '.c-pagination', SortHierarchy);


        })

            .fail((err) => {

                document.getElementById('statusContainer').classList.add('d-block');
                document.getElementById('statusContainer').classList.remove('d-none')

                //$(statusIcon).append(`<span>Error moving to exception.</span>`)

                // NEW
                $('#message_notif').text('Error moving to exception.');

                setTimeout(() => {
                    document.getElementById('statusContainer').classList.remove('d-flex')
                    document.getElementById('statusContainer').classList.add('d-none')
                    $(statusIcon).empty()

                }, 3000)
            })
    }





}


const viewException = (v) => {

}


const searchLogs = (v) => {

    let searchVal = document.getElementById('searchVal').value

    let finalList = []


    let finalRes = logsList.some(val => {


        //Bill no and invoice number is switch by data
        let clientN = val.client_Name != null ? val.client_Name.toLowerCase() : ''
        let invoiceN = val.invoice_Number != null ? val.invoice_Number.toLowerCase() : ''
        let billN = val.bill_No != null ? val.bill_No.toLowerCase() : ''
        let userN = val.user_Name != null ? val.user_Name.toLowerCase() : ''


        if (clientN.includes(searchVal.toLowerCase()) || invoiceN.includes(searchVal.toLowerCase()) || billN.includes(searchVal.toLowerCase()) || userN.includes(searchVal.toLowerCase())) {

            finalList.push(val)
        }
    })


    $(tblBodyLogs).empty();




    finalList.map((i, k) => {

        let htmlData = `
                       <tr>
                            <td>
                                <div class="p-3">
                                  
                                    ${moment(i.date_Log).format('DD MMMM YYYY')}
                                </div>    
                            </td>

                              <td>
                                <div class="p-3">
                                    ${moment(i.time_Log).format('HH:mm')}
                                </div>    
                            </td>

                              <td>
                                <div class="p-3">
                                   ${i.user_Name}
                                </div>    
                            </td>

                              <td>
                                <div class="p-3">
                                    ${i.client_Name}
                                </div>    
                            </td>

                              <td>
                                <div class="p-3">
                                    ${i.invoice_Number}
                                </div>    
                            </td>

                            <td>
                                <div class="p-3">
                                    ${i.bill_No}
                                </div>    
                            </td>

                            

                              <td>
                                <div class="p-3">
                                    ${i.action}
                                </div>    
                            </td>

                                  <td>
                                <div class="p-3 reason-ellipsis">
                                    ${i.previous_Exception_Reason}
                                </div>    
                            </td>

                              <td>
                                <div class="p-3 reason-ellipsis">
                                    ${i.current_Exception_Reason}
                                </div>    
                            </td>

                             <td>
                                <div class="p-3 reason-ellipsis">
                                    ${i.deletion_Reason}
                                </div>    
                            </td>
                       </tr>
                   `
        let val = escapeHtml(htmlData)
        $(tblBodyLogs).append(unescapeHtml(val));
    })
}


const downloadActivityLogs = () => {
    window.location = '/download_viewlogreport_clientengagement'
}

//generate javascript that will get the date on monday next week





