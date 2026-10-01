//let dateSortPrev = "Newest - Oldest";
let dateSortPrev = "";

let pageNumber = 1;
let pageSize = 10;
let dateSentPrev = "";
let clientName = "";
var selectedStartDateU, selectedEndDateU;
let tab = 0;


let selectedUser = ""
let selectedPartner = ""
let selectedSecretary = ""
let selectedItem = ""
let selectedGroupVal = ""
let userRole = -1

let unmutableSelectedSecretary = ""
let unmutableSelectedGroupVal = ""

let countUpdate = 0;

let groupCodeList = []
let groupCodeListUmuttable = []
let adminLogsList = []
let unmutableAdminLogsList = []

let tableList = []
let secretaryTableList = []

let groupEdited = false;
let secretaryEdited = false;

let userState = ''
let editSecretaryModal = false


$('#addUserModal').on('hidden.bs.modal', function () {
    // do something…

    document.getElementById('myInputEngagementUser').value = ''
    document.getElementById('addbutton').disabled = false;
    document.getElementById("regularradio").checked = false
    document.getElementById("adminradio").checked = false

});



$('#editUserModal').on('hidden.bs.modal', function () {
    // do something…

    document.getElementById('myInputEngagementUser').value = ''
    document.getElementById('updateButton').classList.add('disableBtn')
    document.getElementById('updateButton').disabled = true;
    document.getElementById("regularradio").checked = false
    document.getElementById("adminradio").checked = false
    document.getElementById("editRegularRadio").checked = false
    document.getElementById("editAdminRadio").checked = false

});


$('#addSecretaryModal').on('hidden.bs.modal', function () {
    // do something…

    document.getElementById('myInputSecretaryUser').value = ''
    document.getElementById('myInputPartnerUser').value = ''
    document.getElementById('groupInput').value = ''

    document.getElementById('myInputSecretaryUserEdit').value = ''
    document.getElementById('myInputPartnerUserEdit').value = ''
    document.getElementById('groupInputEdit').value = ''

});



$('#editSecretaryModal').on('hidden.bs.modal', function () {
    // do something…

    document.getElementById('myInputSecretaryUserEdit').value = ''
    document.getElementById('myInputPartnerUserEdit').value = ''
    document.getElementById('groupInputEdit').value = ''

    editSecretaryModal = false;
    document.getElementById('myInputSecretaryUser').value = ''
    document.getElementById('myInputPartnerUser').value = ''
    document.getElementById('groupInput').value = ''
});


let picker = $('#PreviouslyDateSentRangeFilter').daterangepicker({
    locale: {
        format: 'DD MMMM YYYY'
    },
    "alwaysShowCalendars": true,
    autoApply: true,
    autoUpdateInput: true,
    "showCustomRangeLabel": false,
},
    function (start, end, label) {
        //
        // Lets update the fields manually this event fires on selection of range
        selectedStartDateU = start.format('DD MMMM YYYY'); // selected start
        selectedEndDateU = end.format('DD MMMM YYYY'); // selected end
    }
);

//pageSize = $('#pageintervalUtil').val();

//const myparam = pageNumber + "|" + pageSize + "|" + clientName + "|" + dateSentPrev + "|" + dateSortPrev;
//loadExceptionList('/Settings/GetData', myparam, '#tblBodyExceptions', '#tblFooterExceptions', '.util-pagination');




$(function () {

    //legacy
    loadUser()
    //for search
    loadUserList()
    //for table
    loadUserListTable()
    //for search secretary
    loadSecretarySearchList()
    //for search partner
    loadPartnerSearchList()
    //load group code list for adding secretary
    getGroupCodeList()



localStorage.setItem('utilTab', 0)




    //#jsonData is rendered by _Layout.cshtml and is always valid JSON here.
    //If it is ever missing the Utilities page still works; only userState stays empty.
    var jsonDataElement = document.getElementById('jsonData');

    if (jsonDataElement && jsonDataElement.value) {
        try {
            userState = JSON.parse(jsonDataElement.value);
        }
        catch (e) {
            console.error('utilities.js - could not parse #jsonData', e);
        }
    }

})

const loadUser = () => {

    let tblString = ''


    tblString = `
        <tr>
            <td style="height: 70px">
                <div class='p-3'>Renz Castaloni</div>
            </td>
             <td style="height: 70px">
                <div class='p-3'>User</div>
            </td>
             <td style="height: 70px">
                <div class='p-3'>01 January 2023</div>
            </td>

             <td style="height: 70px">
                <div class="container d-flex p-3">
                     <span onclick="editUser()" class="Appkit4-icon icon-pencil-fill  a-text-grey"></span>
                     <span class="Appkit4-icon icon-delete-fill ms-2 a-text-grey"></span>
                </div>
            </td>
        </tr>
    `
    let val = escapeHtml(tblString)
    $('#tblUserAccessBody').append(unescapeHtml(val)).html();
}

const openAdminLogs = () => {
    $('#adminLogsModal').modal('show');
    document.getElementById('searchValActivity').value = ""
    if (tab == 0) {



        adminLogs()
    }
    else if (tab == 1) {
        loadSecretaryLogs();
    }




}


const openAddUser = () => {
    validateAccess(0)


    if (tab == 0) {
        $('#addUserModal').modal('show');
    }
    else {
        loadSecretarySearchList()
        //for search partner
        loadPartnerSearchList()

        $('#addSecretaryModal').modal('show');

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
            $(groupList).append(unescapeHtml(val)).html();
        })

    }

}

const closeEditUser = () => {
    $('#editUserModal').modal('hide');
    document.getElementById('updateButton').disabled = true;

    document.getElementById('updateButton').classList.add('disableBtn')
}

const editUser = (v) => {


    validateAccess(0)

    $.getJSON('/view_role', {
        I_variable: v

    }, (v) => {
        $('#editUserModal').modal('show');
        selectedUser = v[0]
        userRole = v[0].user_Role


        if (v[0].user_Role == 0) {
            document.getElementById('editRegularRadio').checked = true
            document.getElementById('roleEdit').value = "User"
        }
        else {

            document.getElementById('editAdminRadio').checked = true
            document.getElementById('roleEdit').value = "Admin"
        }

        document.getElementById('nameEdit').value = v[0].employee_Name
        document.getElementById('dateEdit').value = v[0].date_Assigned

    })



}

const selectTab = (v) => {
    document.getElementById('searchVal').value = ''
    if (v == 0) {

        loadUserListTable()
        document.getElementById('addUserButton').textContent = "Add user"
        document.getElementById('logsLabel').textContent = "Admin logs"
        document.getElementById('logs').textContent = "Admin logs"


        document.getElementById('useraccessTable').classList.remove('d-none')
        //document.getElementById('useraccessTable').classList.add('d-flex')

        document.getElementById('secretaryaccessTable').classList.remove('d-flex')
        document.getElementById('secretaryaccessTable').classList.add('d-none')

        document.getElementById('secretarylistbutton').classList.remove('active-link')
        document.getElementById('userlistbutton').classList.add('active-link')

        document.getElementById('userlistbutton').style.borderBottom = '3px solid'
        document.getElementById('secretarylistbutton').style.borderBottom = '0px solid'

        
        
        document.getElementById('label-user-title').textContent = "User access management"
        document.getElementById('label-user').textContent = "View and manage user access"

      


    }
    else {
        loadSecretaryListTable()
        document.getElementById('logsLabel').textContent = "Activity logs"

        document.getElementById('addUserButton').textContent = "Add secretary"
        document.getElementById('logs').textContent = "Activity logs"

        document.getElementById('useraccessTable').classList.add('d-none')
        document.getElementById('useraccessTable').classList.remove('d-flex')

        //document.getElementById('secretaryaccessTable').classList.add('d-flex')
        document.getElementById('secretaryaccessTable').classList.remove('d-none')

        document.getElementById('secretarylistbutton').classList.add('active-link')
        document.getElementById('userlistbutton').classList.remove('active-link')

        document.getElementById('userlistbutton').style.borderBottom = '0px solid'
        document.getElementById('secretarylistbutton').style.borderBottom = '3px solid'

        document.getElementById('label-user-title').textContent = "Secretary list"
        document.getElementById('label-user').textContent = "View and manage secretary list"

       
    }
    tab = v

    //getting value from local storage of util tab
    localStorage.setItem('utilTab', v)


}


const loadSecretary = () => {

}
//previos function
//$(function () {

//    $('#PreviouslyDateSentRangeFilter').val('');

//    $("#tblmyExceptions th").css("background-color", "white");
//    $("#tblmyExceptions th").css("position", "relative");
//    $("#tblmyExceptions th").css("z-index", "1000");
//    $("#tblmyExceptions th").css("border-bottom", "1px solid black");

//    var $th1 = $('.tableFixHeadExceptions').find('thead th')
//    $('.tableFixHeadExceptions').on('scroll', function () {
//        $th1.css('transform', 'translateY(' + this.scrollTop + 'px)');
//    });


//    function GetListU() {
//        //pageSize = $('#pageintervalUtil').val();
//        pageNumber = 1;
//        clientName = $('#RefClientName').val();

//        const myparam = pageNumber + "|" + pageSize + "|" + clientName + "|" + dateSentPrev + "|" + dateSortPrev;
//        loadExceptionList('/Settings/GetData', myparam, '#tblBodyExceptions', '#tblFooterExceptions', '.util-pagination');
//    }


//    $('#BtnSubmitReference').on('click', function () {
//        GetListU();
//        return false;
//    });


//    $(document).on('change', '#pageintervalUtil', function (event) {
//        GetListU();
//    });


//    $('#btnApplyFilterPreviouslyDateSent').on('click', function () {
//        dateSentPrev = $('#PreviouslyDateSentRangeFilter').val();
//        dateSortPrev = $('input[name="PreviouslyDateSentListValue"]:checked').val();

//        if (typeof (selectedStartDateU) == "undefined") {
//            selectedStartDateU = moment().format('DD MMMM YYYY'); // selected start
//            selectedEndDateU = moment().format('DD MMMM YYYY'); // selected end
//        }

//        //

//        if ((dateSentPrev.trim() != "" && dateSentPrev.trim() != "") || typeof (dateSortPrev) != "undefined" && dateSortPrev != "") {

//            $('#PreviouslyDateSentFilter').children().children('.filter-img').removeClass('disable');
//            $('#PreviouslyDateSentFilter').children().children('.filter-img').addClass('active');
//        } else {
//            dateSentPrev = "";
//            $('#PreviouslyDateSentFilter').children().children('.filter-img').removeClass('active');
//            $('#PreviouslyDateSentFilter').children().children('.filter-img').addClass('disable');
//        }

//        $('#PreviouslyDateSentFilterPanel').hide();

//        GetListU();

//        AddFilter();
//        return false;
//    });

//    $(document).on('click', '.util-pagination a', function (event) {

//        event.preventDefault();
//        let mydatefrom, mydateto;

//        var page = $(this).attr('href').split('list=')[1];
//        if (typeof page == 'undefined') {
//            return;
//        }

//        $('.util-pagination li').removeClass('active');
//        $(this).parent('li').addClass('active');


//        if ($('#PreviouslyDateSentRangeFilter').val() == '') {
//            mydatefrom = '';
//            mydateto = '';
//        } else {
//            mydatefrom = selectedStartDateU;
//            mydateto = selectedEndDateU;
//        }

//        pageNumber = page;

//        const myparam = pageNumber + "|" + pageSize + "|" + clientName + "|" + dateSentPrev + "|" + dateSortPrev;
//        loadExceptionList('/Settings/GetData', myparam, '#tblBodyExceptions', '#tblFooterExceptions', '.util-pagination');


//    });

//    function AddFilter() {
//        let myFilter = '';
//        $('#myFiltersUtil').empty();

//        if (dateSentPrev != "" && typeof (dateSentPrev) != "undefined") {

//            myFilter += '<div class="filter-container"><div class="d-flex">Previously sent date: <div class="filter-items-2 ml-1" title="' + (dateSentPrev) + '">' + (dateSentPrev) + '</div><button id="removeDateFilter" class="btn filter-dispose-btn" onclick="removeFilter(this)">x</button></div></div>';

//        }
//        if (dateSortPrev != "" && typeof (dateSortPrev) != "undefined") {

//            myFilter += '<div class="filter-container"><div class="d-flex">Date sort: <div class="filter-items-2 ml-1" title="' + dateSortPrev + '">' + dateSortPrev + '</div><button id="removeDateSortFilter" class="btn filter-dispose-btn" onclick="removeFilter(this)">x</button></div></div>';

//        }
//        $('#myFiltersUtil').append(myFilter);
//    }

//    $('#BtnAddAccount').on('click', function () {

//        $('#ModalAddException').modal('show');
//    });

//    $('#ClientName').autocomplete({
//        source: function (request, response) {

//            var param = bindToken({ ClientName: $('#ClientName').val() });
//            $.ajax({
//                url: "/Settings/GetClients",
//                data: param,
//                dataType: "json",
//                type: "POST",
//                contentType: "application/x-www-form-urlencoded",
//                dataFilter: function (data) { return data; },
//                success: function (data) {
//                    //
//                    response($.map(data, function (item) {
//                        return {
//                            label: item.clientName + " | " + item.clientId,
//                            value: item.clientName,
//                            data: item.clientName + " | " + item.clientId
//                        }
//                    }))

//                },
//                error: function (XMLHttpRequest, textStatus, errorThrown) {
//                    //var err = eval(XMLHttpRequest.responseText);
//                    alert(XMLHttpRequest.responseText)
//                }
//            });
//        },
//        minLength: 1, //This is the Char length of inputTextBox    
//       select: function (event, ui) {
//           var resArr;
//           //
//            resArr = ui.item.data.split("|");

//           $('#ClientName').val(resArr[0]);
//           $('#ClientCode').val(resArr[1]);

//        }

//    });


//    $('#ClientName').on('focusout', function () {
//        let myvalue = $(this).val();
//        if (myvalue.length <= 0) {
//            $('#ClientCode').val('');
//        }
//    });

//    $('#BtnDownload').on('click', function () {

//        let data = clientName + "|" + dateSentPrev;

//        window.open("/Report/DownloadExceptions?file=" + data, '_blank');
//    });

//});



function removeFilter(e) {

    $(e).parent().parent().remove();

    switch (e.id) {
        case "removeDateFilter":
            $('#PreviouslyDateSentRangeFilter').val('');

            $('#btnApplyFilterPreviouslyDateSent').trigger('click');

            break;

        default:
            dateSortPrev = '';

            $('input[name="PreviouslyDateSentListValue"]').each(function () {
                this.checked = false;
            });

            $('#btnApplyFilterPreviouslyDateSent').trigger('click');

            break;
    }

}

function RemoveClientToException(e) {
    let tr = $(e).closest('tr');
    let refno = tr.find('td:first-child .refNo').val();

    RemoveThisClientToException(refno);
    //$.ajax({
    //    headers: {
    //        'X-CSRF-TOKEN': $('input[name="__RequestVerificationToken"]').val()
    //        , 'Content-Type': 'application/x-www-form-urlencoded'
    //    },
    //    type: 'DELETE',
    //    url: '/Settings/RemoveException?Reference=' + refno,
    //    dataType: 'json',
    //    success: function (data) {
    //        //$('#BtnSubmitReference').trigger('click');

    //        const myparam = pageNumber + "|" + pageSize + "|" + clientName + "|" + dateSentPrev + "|" + dateSortPrev;
    //        loadExceptionList('/Settings/GetData', myparam, '#tblBodyExceptions', '#tblFooterExceptions', '.util-pagination');

    //    },
    //    error: function (xhr, status, error) {
    //        var errorMessage = xhr.status + ': ' + xhr.statusText
    //        alert('Error - ' + errorMessage);
    //    }
    //});


}

//function ShowExceptionInfo(e) {
//    let tr = $(e).closest('tr');
//    let refId = tr.find('td:first-child .refNo').val();
//    let refreason = tr.find('td:first-child .refReasons').val();
//    let dclientcode = tr.find('td:eq(0)').text();
//    let dclientname = tr.find('td:eq(1)').text();

//    $('#displayClientName').val(dclientname);
//    $('#displayCode').val(dclientcode);
//    $('#displayReason').val(refreason);
//    $('#myRefToRemove').val(refId);


//    $('#ModalShowException').modal('show');


//    //$('#ClientName').val(dclientname);
//    //$('#ClientCode').val(dclientcode);
//    //$('#Reasons').val(refreason);

//    //$('#ModalAddException').modal('show');


//    return false;
//}


function RemoveThis() {

    let refno = $('#myRefToRemove').val();


    RemoveThisClientToException(refno);

    $('#myRefToRemove').val('');

    $('#ModalShowException').modal('toggle');

    return false;
}

function RemoveThisClientToException(refno) {
    $.ajax({
        headers: {
            'X-CSRF-TOKEN': $('input[name="__RequestVerificationToken"]').val(),
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        type: 'DELETE',
        url: '/Settings/RemoveException?Reference=' + refno,
        dataType: 'json',
        success: function (data) {
            const myparam = pageNumber + "|" + pageSize + "|" + clientName + "|" + dateSentPrev + "|" + dateSortPrev;
            loadExceptionList('/Settings/GetData', myparam, '#tblBodyExceptions', '#tblFooterExceptions', '.util-pagination');

        },
        error: function (xhr, status, error) {
            var errorMessage = xhr.status + ': ' + xhr.statusText;
            alert('Error - ' + errorMessage);
        }
    });
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

const loadUserList = () => {

    $.getJSON('/stafflist', {

    }, (v) => {


        $(engagementuserList).empty();

        //<a href='Select group' class="dropdown-item" >Select group</div>
        let html = ``
        //$(groupList).append(html);

        v.map((i, k) => {

            let finalVal = JSON.stringify(i)
            let html = `    
                      <a target="_blank" class="dropdown-item" onclick='return selectedSearchUser(${finalVal}, 0)'>${i.name}</a>
                    `

            let val = escapeHtml(html)
            $(engagementuserList).append(unescapeHtml(val)).html();
        })
    })
}

const loadSecretarySearchList = () => {

    $.getJSON('/stafflist_executive', {

    }, (v) => {


        $(secretaryuserList).empty();
        $(secretaryuserListEdit).empty();

        //<a href='Select group' class="dropdown-item" >Select group</div>
        let html = ``
        //$(groupList).append(html);

        v.map((i, k) => {

            let finalVal = JSON.stringify(i)
            let html = `    
                      <a target="_blank" class="dropdown-item" onclick='return selectedSearchUser(${finalVal}, 1)'>${i.name}</a>
                    `

            let val = escapeHtml(html)
            $(secretaryuserList).append(unescapeHtml(val)).html();
            $(secretaryuserListEdit).append(unescapeHtml(val)).html();
        })
    })
}


const loadPartnerSearchList = () => {

    $.getJSON('/stafflist_partner', {

    }, (v) => {


        $(partneruserList).empty();
        $(partneruserListEdit).empty();

        //<a href='Select group' class="dropdown-item" >Select group</div>
        let html = ``
        //$(groupList).append(html);

        v.map((i, k) => {

            let finalVal = JSON.stringify(i)
            let html = `    
                      <a target="_blank" class="dropdown-item" onclick='return selectedSearchUser(${finalVal}, 2)'>${i.name}</a>
                    `
            let val = escapeHtml(html)

            $(partneruserList).append(unescapeHtml(val)).html();
            $(partneruserListEdit).append(unescapeHtml(val)).html();
        })
    })
}


const selectedSearchUser = (v, type) => {

    //type
    //0 admin
    // 1 secretary
    // 2 partner


    if (type == 0) {
        document.getElementById('myInputEngagementUser').value = v.name
        selectedUser = v
    }
    else if (type == 1) {
        document.getElementById('myInputSecretaryUser').value = v.name
        document.getElementById('myInputSecretaryUserEdit').value = v.name
        selectedSecretary = v

        countUpdate = countUpdate + 1;
        secretaryEdited = true;
        //remove dsiable button




        $.getJSON('/stafflist_partner', {
            i_variable: v.id
        }, (v) => {


            $(partneruserList).empty();

            //<a href='Select group' class="dropdown-item" >Select group</div>
            let html = ``
            //$(groupList).append(html);

            v.map((i, k) => {

                let finalVal = JSON.stringify(i)
                let html = `    
                          <a target="_blank" class="dropdown-item" onclick='return selectedSearchUser(${finalVal}, 2)'>${i.name}</a>
                        `

                let val = escapeHtml(html)
                $(partneruserList).append(unescapeHtml(val)).html();
            })
        })

    }
    else if (type == 2) {
        document.getElementById('myInputPartnerUser').value = v.name
        document.getElementById('myInputPartnerUserEdit').value = v.name
        selectedPartner = v

        $.getJSON('/stafflist_executive', {
            i_variable: v.id
        }, (v) => {


            $(secretaryuserList).empty();

            //<a href='Select group' class="dropdown-item" >Select group</div>
            let html = ``
            //$(groupList).append(html);

            v.map((i, k) => {

                let finalVal = JSON.stringify(i)
                let html = `    
                          <a target="_blank" class="dropdown-item" onclick='return selectedSearchUser(${finalVal}, 1)'>${i.name}</a>
                        `
                let val = escapeHtml(html)
                $(secretaryuserList).append(unescapeHtml(val)).html();
            })
        })

    }



    removeDisable(type, v)
}

const selectedRole = (v) => {



    document.getElementById('updateButton').disabled = false;
    document.getElementById('addbutton').disabled = false;

    document.getElementById('updateButton').classList.remove('disableBtn')
    document.getElementById('addbutton').classList.remove('disableBtn')


    if (v == "regularradio" || v == "editRegularRadio") {
        document.getElementById(v).checked = true
        document.getElementById('adminradio').checked = false
        document.getElementById('editAdminRadio').checked = false
        userRole = 0
    }
    else {

        document.getElementById(v).checked = true
        document.getElementById('regularradio').checked = false
        document.getElementById('editRegularRadio').checked = false
        userRole = 1
    }

    removeDisable()
}

const updateUserRole = () => {




    if (tab == 0) {
        document.getElementById('updateButton').disabled = true;

        $.getJSON('/update_role', {
            id: selectedUser.id,
            employee_code: selectedUser.employee_Code,
            employee_name: selectedUser.name,
            user_Role: userRole,
            action_taker: document.getElementById('username').innerText,
            action_taken: 'Updated role of ' + selectedUser.employee_Name + (userRole == 0 ? " as regular user." : " as admin.")
        }, (v) => {

            document.getElementById('statusContainer').classList.remove('d-none')
            document.getElementById('statusContainer').classList.add('d-block');

            //$(statusIcon).append(`<span class='a-text-white ml-3'> User successfully updated!</span>`)

            // NEW
            $('#message_notif').text('User successfully updated!');

            setTimeout(() => {
                document.getElementById('statusContainer').classList.add('d-none')
                document.getElementById('statusContainer').classList.remove('d-flex')

                $(statusIcon).empty();
            }, 3000)

            document.getElementById('myInputEngagementUser').value = ''
            document.getElementById('updateButton').classList.add('disableBtn')
            document.getElementById('updateButton').disabled = true;
            document.getElementById("regularradio").checked = false
            document.getElementById("adminradio").checked = false
            document.getElementById("editRegularRadio").checked = false
            document.getElementById("editAdminRadio").checked = false
            selectedUser = ''
            userRole = ''
            $('#editUserModal').modal('hide');

            loadUserListTable()

        })
            .fail((err) => {
                document.getElementById('statusContainer').classList.remove('d-none')
                document.getElementById('statusContainer').classList.add('d-block');
                document.getElementById('statusContainer').style.backgroundColor = "red"
                document.getElementById('statusIcon').classList.remove('icon-circle-checkmark-fill')
                document.getElementById('statusIcon').classList.add('icon-circle-delete-fill')

                document.getElementById('updateButton').disabled = true;
                document.getElementById('updateButton').classList.add('disableBtn')

                // $(statusIcon).append(`<span class='a-text-white'>Unable to update, Please try again.</span>`)

                // NEW
                $('#message_notif').text("Unable to update, Please try again.");

                document.getElementById('addbutton').disabled = false;
                document.getElementById("regularradio").checked = false
                document.getElementById("adminradio").checked = false
                document.getElementById("editRegularRadio").checked = false
                document.getElementById("editAdminRadio").checked = false

                setTimeout(() => {
                    document.getElementById('statusContainer').classList.add('d-none')
                    document.getElementById('statusContainer').classList.remove('d-flex')
                    document.getElementById('statusContainer').style.backgroundColor = "green"
                    document.getElementById('statusIcon').classList.add('icon-circle-checkmark-fill')

                    document.getElementById('statusIcon').classList.remove('icon-circle-delete-fill')

                    $(statusIcon).empty()
                }, 3000)


                selectedUser = ''
                userRole = ''
                $('#editUserModal').modal('hide');
            })
    }
    else if (tab == 1) {

        //need to fix this properties

        let taker = userState.employee_Name



        let action

      
        if (secretaryEdited && !groupEdited) {
            //if secretary only changed
            action = "Changed " + selectedPartner.name + "'s secretary to " + selectedSecretary.name + " for " + selectedGroupVal.group
        }
        else if (groupEdited && !secretaryEdited) {
            //if group only changed
            action = "Changed " + selectedPartner.name + " and " + selectedSecretary.name + "'s group to " + selectedGroupVal.description
        }
        else if (groupEdited && secretaryEdited) {
            // if both
            action = "Changed " + selectedPartner.name + "'s secretary to " + selectedSecretary.name + " and group to " + selectedGroupVal.description
        }

       


        

        $.getJSON('/validate_engagement_secretary', {
            secretary_variable: selectedSecretary.id,
            partner_variable: selectedPartner.id,
            group_variable: selectedGroupVal.group_Code_WO_Desc,

        }, (data) => {

            if (data.length != 0) {
                let getCounter = data[0].engagement_Secretary_Count

                if (getCounter >= 1) {
                    alert('Partner with the same group already exists. Please try again.')
                }
                else {

                    $.getJSON('/update_engagement_secretary', {
                        secretary: selectedSecretary.id,
                        partner: selectedPartner.id,
                        group: selectedGroupVal.group_Code_WO_Desc,
                        Id: selectedItem,
                        action_Taker: userState.username,
                        action_Taken: action

                    }, (data) => {
                        document.getElementById('searchVal').value = ""
                        searchFromList()
                        document.getElementById('statusContainer').classList.remove('d-none')
                        document.getElementById('statusContainer').classList.add('d-block');
                        groupEdited = false;
                        secretaryEdited = false;

                        //$(statusIcon).append(`<span class='a-text-white ml-3'> Secretary successfully updated!</span>`)

                        // NEW
                        $('#message_notif').text("Secretary successfully updated!");

                        setTimeout(() => {
                            document.getElementById('statusContainer').classList.add('d-none')
                            document.getElementById('statusContainer').classList.remove('d-flex')




                            document.getElementById('editbuttonSecretary').classList.add('disableBtn')
                            document.getElementById('editbuttonSecretary').disabled = true

                            $(statusIcon).empty();
                        }, 3000)

                        document.getElementById('myInputSecretaryUserEdit').value = ''
                        document.getElementById('myInputPartnerUserEdit').value = ''
                        document.getElementById('groupInputEdit').value =

                            document.getElementById('myInputSecretaryUser').value = ''
                        document.getElementById('myInputPartnerUser').value = ''
                        document.getElementById('groupInput').value = ''


                        document.getElementById('myInputEngagementUser').value = ''
                        document.getElementById('addbutton').disabled = false;
                        document.getElementById("regularradio").checked = false
                        document.getElementById("adminradio").checked = false

                        loadSecretaryListTable();


                        $('#editSecretaryModal').modal('hide');
                        editSecretaryModal = false;

                        selectedSecretary = '';
                        selectedPartner = '';
                        selectedGroupVal = '';
                        selectedItem = '';

                    }).fail((err) => {
                        document.getElementById('statusContainer').classList.remove('d-none')
                        document.getElementById('statusContainer').classList.add('d-block');
                        document.getElementById('statusContainer').style.backgroundColor = "red"
                        document.getElementById('statusIcon').classList.remove('icon-circle-checkmark-fill')
                        document.getElementById('statusIcon').classList.add('icon-circle-delete-fill')

                        document.getElementById('addbutton').disabled = false;

                        //$(statusIcon).append(`<span class='a-text-white'>Unable to update, Please try again.</span>`)

                        // NEW
                        $('#message_notif').text("Unable to update, Please try again.");


                        setTimeout(() => {
                            document.getElementById('statusContainer').classList.add('d-none')
                            document.getElementById('statusContainer').classList.remove('d-flex')
                            document.getElementById('statusContainer').style.backgroundColor = "green"
                            document.getElementById('statusIcon').classList.add('icon-circle-checkmark-fill')

                            document.getElementById('statusIcon').classList.remove('icon-circle-delete-fill')

                            $(statusIcon).empty()
                        }, 3000)


                        selectedSecretary = '';
                        selectedPartner = '';
                        selectedGroupVal = '';
                        selectedItem = '';

                        $('#editSecretaryModal').modal('hide');
                        editSecretaryModal = false;
                    })
                }
            }
        }
        )
    }


}


const addUserRole = () => {

    if (tab == 0) {
        document.getElementById('addbutton').disabled = true;

        $.getJSON('/add_the_role', {
            employee_code: selectedUser.id,
            employee_name: selectedUser.name,
            user_Role: userRole,
            action_taker: document.getElementById('username').innerText,
            action_taken: 'Added ' + selectedUser.name + (userRole == 1 ? " as admin." : " as regular user.")
        }, (v) => {

            document.getElementById('statusContainer').classList.remove('d-none')
            document.getElementById('statusContainer').classList.add('d-block');

            // $(statusIcon).append(`<span class='a-text-white ml-3'> User successfully added!</span>`)

            // NEW
            $('#message_notif').text("User successfully added!");

            setTimeout(() => {
                document.getElementById('statusContainer').classList.add('d-none')
                document.getElementById('statusContainer').classList.remove('d-flex')

                $(statusIcon).empty();
            }, 3000)

            document.getElementById('myInputEngagementUser').value = ''
            document.getElementById('addbutton').disabled = false;
            document.getElementById("regularradio").checked = false
            document.getElementById("adminradio").checked = false
            selectedUser = ''
            userRole = ''
            $('#addUserModal').modal('hide');

            loadUserList()

            loadUserListTable()

        })
            .fail((err) => {
                document.getElementById('statusContainer').classList.remove('d-none')
                document.getElementById('statusContainer').classList.add('d-block');
                document.getElementById('statusContainer').style.backgroundColor = "red"
                document.getElementById('statusIcon').classList.remove('icon-circle-checkmark-fill')
                document.getElementById('statusIcon').classList.add('icon-circle-delete-fill')

                document.getElementById('addbutton').disabled = false;

                //$(statusIcon).append(`<span class='a-text-white'>Unable to add, Please try again.</span>`)

                // NEW
                $('#message_notif').text("Unable to add, Please try again.");


                setTimeout(() => {
                    document.getElementById('statusContainer').classList.add('d-none')
                    document.getElementById('statusContainer').classList.remove('d-flex')
                    document.getElementById('statusContainer').style.backgroundColor = "green"
                    document.getElementById('statusIcon').classList.add('icon-circle-checkmark-fill')

                    document.getElementById('statusIcon').classList.remove('icon-circle-delete-fill')

                    $(statusIcon).empty()
                }, 3000)


                selectedUser = ''
                userRole = ''
                $('#addUserModal').modal('hide');
            })
    }
    else if (tab == 1) {
        document.getElementById('addbuttonSecretary').disabled = true;
        document.getElementById('addbuttonSecretary').classList.add('disableBtn')


        addSecretary();
    }
}


const closeAddUser = () => {

   
    if (tab == 0) {
        document.getElementById('myInputEngagementUser').value = ''
        document.getElementById("regularradio").checked = false
        document.getElementById("adminradio").checked = false
        selectedUser = ''
        userRole = ''

        document.getElementById('addbutton').classList.add('disableBtn')
        document.getElementById('addbutton').disabled = true

        $('#addUserModal').modal('hide');
    }
    else if (tab == 1) {


        

        if (secretaryEdited && editSecretaryModal) {
            $('#editSecretaryModal').modal('hide');
            editSecretaryModal = false;
            setTimeout(() => {
                $('#ModalEdiSecretaryConfirm').modal('show');
            }, 500)
            
           
        }
        else {
            $('#addSecretaryModal').modal('hide');
            $('#editSecretaryModal').modal('hide');
            editSecretaryModal = false;

            document.getElementById('myInputSecretaryUser').value = ''
            document.getElementById('myInputPartnerUser').value = ''
            document.getElementById('groupInput').value = ''

            document.getElementById('addbutton').disabled = true;

            groupEdited = false;
            secretaryEdited = false;

            document.getElementById('editbuttonSecretary').classList.add('disableBtn')
            document.getElementById('editbuttonSecretary').disabled = true
        }

        

    }

}

const continueEditUser = () => {

    $('#ModalEdiSecretaryConfirm').modal('hide');
    setTimeout(() => {

        
     
        $('#editSecretaryModal').modal('show');
        editSecretaryModal = true;
        document.getElementById('myInputSecretaryUserEdit').value = selectedSecretary.name
        document.getElementById('myInputPartnerUserEdit').value = selectedPartner.partner
        document.getElementById('groupInputEdit').value = selectedGroupVal.group == undefined ? selectedGroupVal.description : selectedGroupVal.group
    }, 500)

}

const confirmCancelEditUser = () => {
    secretaryEdited = false;
    editSecretaryModal = false;

    $('#addSecretaryModal').modal('hide');
    $('#editSecretaryModal').modal('hide');
    $('#ModalEdiSecretaryConfirm').modal('hide');


    document.getElementById('myInputSecretaryUser').value = ''
    document.getElementById('myInputPartnerUser').value = ''
    document.getElementById('groupInput').value = ''

    document.getElementById('addbutton').disabled = true;

    groupEdited = false;
    

    document.getElementById('editbuttonSecretary').classList.add('disableBtn')
    document.getElementById('editbuttonSecretary').disabled = true
}


const loadUserListTable = () => {

    $.getJSON('/load_role', {
    }, (v) => {


        tableList = v
         $(tblUserAccessBody).empty();
        $(tblSecretaryAccessBody).empty();


        let myrow = '';
        //
        v.forEach((d) => {

            let finalVal = JSON.stringify(d)
            myrow += `<tr>
                               
                               <td style="height: 70px">
                                     <div class="container row-max-height p3">
                                        ${d.employee_Name}
                                     </div>
                               </td>
                               <td style="height: 70px"> 
                                    
                                     <div class="container row-max-height p3">
                                      ${d.user_Role == 0 ? "Regular user" : "Admin"}
                                     </div>
                               </td>
                               
                               <td style="height: 70px">
                                     <div class="container row-max-height p3">
                                     ${moment(d.date_Assigned).format('DD MMMM YYYY')}
                                     </div>
                               </td>
                               
                               <td style="height: 70px"> 
                                    <div class="container row-max-height d-flex p-3">
                                         <span onclick='editUser("${d.id}")' class="Appkit4-icon icon-pencil-fill  a-text-grey"></span>
                                        <span onclick='deleteUser(${finalVal}, "engagement")' class="Appkit4-icon icon-delete-fill ms-2 a-text-grey"></span>
                                    </div>
                               </td>
                           </tr>`
        });
        let val = escapeHtml(myrow)
        $('#tblUserAccessBody').append(unescapeHtml(val)).html();


    })
}




const deleteUser = (v) => {


    validateAccess(0)
    selectedUser = v


    if (tab == 0) {
        document.getElementById('eName').innerText = v.employee_Name
        document.getElementById('userrole').innerText = v.user_Role == 0 ? "user" : "admin"
        
        document.getElementById('label-remove').innerText = "Delete user"
        document.getElementById('SubmitException').innerText = "Yes, remove user"
    }
    if (tab == 1) {
        document.getElementById('eName').innerText = v.secretary
        document.getElementById('userrole').innerText = "Secretary"

        document.getElementById('label-remove').innerText = "Delete secretary"

        document.getElementById('SubmitException').innerText = "Yes, remove"
    }

    $('#ModalRemoveUser').modal('show');



}


const confirmDeleteUser = () => {


    if (tab == 0) {

        $.getJSON('/delete_role', {
            Id: selectedUser.id,
            Action_Taker: document.getElementById('username').innerText,
            Action_Taken: 'Removed ' + selectedUser.employee_Name + (selectedUser.user_Role == 1 ? " as admin." : " as regular user.")
        }, (v) => {


            document.getElementById('statusContainer').classList.remove('d-none')
            document.getElementById('statusContainer').classList.add('d-block');

            //$(statusIcon).append(`<span class='a-text-white ml-3'> User successfully removed!</span>`)

            // NEW
            $('#message_notif').text('User successfully removed!');

            setTimeout(() => {
                document.getElementById('statusContainer').classList.add('d-none')
                document.getElementById('statusContainer').classList.remove('d-flex')

                $(statusIcon).empty();

            }, 3000)

            selectedUser = ''
            userRole = ''
            $('#ModalRemoveUser').modal('hide');
            loadUserList();
            loadUserListTable();
        })

            .fail((err) => {
                document.getElementById('statusContainer').classList.remove('d-none')
                document.getElementById('statusContainer').classList.add('d-block');
                document.getElementById('statusContainer').style.backgroundColor = "red"
                document.getElementById('statusIcon').classList.remove('icon-circle-checkmark-fill')
                document.getElementById('statusIcon').classList.add('icon-circle-delete-fill')


                //$(statusContainer).append(`<span class='a-text-white'>Unable to delete, Please try again.</span>`)

                // NEW
                $('#message_notif').text("Unable to delete, Please try again.");


                setTimeout(() => {
                    document.getElementById('statusContainer').classList.add('d-none')
                    document.getElementById('statusContainer').classList.remove('d-flex')
                    document.getElementById('statusContainer').style.backgroundColor = "green"
                    document.getElementById('statusIcon').classList.add('icon-circle-checkmark-fill')
                    document.getElementById('statusIcon').classList.remove('icon-circle-delete-fill')

                    $(statusIcon).empty()

                }, 3000)
            })
    } else if (tab == 1) {

        deleteEngagement()

    }

}


const adminLogs = () => {

    $(tblBodyLogs).empty();
    validateAccess(0)
    $.getJSON('/admin_logs', {
    }, (v) => {

        adminLogsList = v
        unmutableAdminLogsList = v

        let tblString = ''


        v.map((i, k) => {
            tblString = `
        <tr>
            <td>
                <div class='p-3'>${moment(i.log_Date).format('DD MMMM YYYY')}</div>
            </td>
             <td>
                <div class='p-3'>${i.log_Time_Format}</div>
            </td>
             <td>
                <div class='p-3'>${i.action_Taker}</div>
            </td>

             <td>
                <div>
                   ${i.action_Taken}
                </div>
            </td>
        </tr>
    `
            let val = escapeHtml(tblString)
            $('#tblBodyLogs').append(unescapeHtml(val)).html()
        })


    })

        .fail((err) => {

        })
}

const searchAdminLogs = () => {

    let searchVal = document.getElementById('searchValActivity').value

    let finalList = []

    let finalRes = adminLogsList.some(val => {




        let actionTaker = val.action_Taker != null ? val.action_Taker.toLowerCase() : ''

        if (actionTaker.includes(searchVal.toLowerCase()) || actionTaker.includes(searchVal.toLowerCase())) {

            finalList.push(val)
        }
    })

    $(tblBodyLogs).empty()


    finalList.map((i, k) => {
        tblString = `
        <tr>
            <td>
                <div class='p-3'>${moment(i.log_Date).format('DD MMMM YYYY')}</div>
            </td>
             <td>
                <div class='p-3'>${i.log_Time_Format}</div>
            </td>
             <td>
                <div class='p-3'>${i.action_Taker}</div>
            </td>

             <td>
                <div>
                   ${i.action_Taken}
                </div>
            </td>
        </tr>
    `
        let val = escapeHtml(tblString)
        $('#tblBodyLogs').append(unescapeHtml(val)).html()
    })
}

const searchFromList = () => {

    let searchVal = document.getElementById('searchVal').value

    if (tab == 0) {



        let finalList = []

        let finalRes = tableList.some(val => {



            let roleV = val.user_Role.toLowerCase()
            let employeeV = val.employee_Name.toLowerCase()


            if (roleV.includes(searchVal.toLowerCase()) || employeeV.includes(searchVal.toLowerCase())) {

                finalList.push(val)
            }
        })


        $(tblUserAccessBody).empty();
        $(tblSecretaryAccessBody).empty();

        let myrow = '';
        //
        finalList.forEach((d) => {

            let finalVal = JSON.stringify(d)
            myrow += `<tr>
                               <td style="height: 70px; padding: 20px">
                                     <div class="">
                                        ${d.employee_Name}
                                     </div>
                               </td >
                               <td style="height: 70px; padding: 20px"> 
                                    
                                     <div class="">
                                      ${d.user_Role == 0 ? "Regular user" : "Admin"}
                                     </div>
                               </td>
                               
                               <td style="height: 70px; padding: 20px">
                                     <div class="">
                                     ${moment(d.date_Assigned).format('DD MMMM YYYY')}
                                     </div>
                               </td>
                               
                               <td style="height: 70px; padding: 20px"> 
                                    <div class="">
                                         <span onclick='editUser("${d.id}")' class="Appkit4-icon icon-pencil-fill  a-text-grey"></span>
                                        <span onclick='deleteUser(${finalVal}, "engagement")' class="Appkit4-icon icon-delete-fill ms-2 a-text-grey"></span>
                                    </div>
                               </td>
                           </tr>`
        });
        let val = escapeHtml(myrow)

        $('#tblUserAccessBody').append(unescapeHtml(val)).html();
    }
    else if (tab == 1) {
        $(tblUserAccessBody).empty();
        $(tblSecretaryAccessBody).empty();

        let finalList = []

        let finalRes = secretaryTableList.some(val => {




            let partner = val.partner == null ? "" : val.partner.toLowerCase()
            let secretary = val.secretary == null ? "" : val.secretary.toLowerCase()
            let group = val.group == null ? "" : val.group.toLowerCase()



            if (secretary.includes(searchVal.toLowerCase()) || group.includes(searchVal.toLowerCase()) || partner.includes(searchVal.toLowerCase())) {

                finalList.push(val)
            }
        })


       


        let myrow = '';
        //
        finalList.forEach((d) => {

            let finalVal = JSON.stringify(d)
            myrow += `<tr>
                               
            <td style="height: 70px; padding: 20px">
                  <div class="">
                     ${d.secretary}
                  </div>
            </td>
            <td style="height: 70px; padding: 20px"> 
                 
                  <div class="">
                   ${d.partner}
                  </div>
            </td>
            
            <td style="height: 70px; padding: 20px">
                  <div class="">
                  ${d.group}
                  </div>
            </td>
            
            <td style="height: 70px; padding: 20px"> 
                 <div class="">
                      <span onclick='viewEngagementSecretary(${finalVal})' class="Appkit4-icon icon-pencil-fill  a-text-grey"></span>
                     <span onclick='deleteUser(${finalVal}, "secretary")' class="Appkit4-icon icon-delete-fill ms-2 a-text-grey"></span>
                 </div>
            </td>
        </tr>`
        });

        let val = escapeHtml(myrow)

        $('#tblSecretaryAccessBody').append(unescapeHtml(val)).html();

    }

}

function filterFunction(inputId, dropdownId) {

    document.getElementById(dropdownId).style.display = 'block';

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



const removeDisable = (type, v, label) => {







    if (type == 0) {
        if (userRole != -1 && selectedUser != "") {


            document.getElementById('addbutton').classList.remove('disableBtn')
            document.getElementById('addbutton').disabled = false

        }
        else {

        }
    }


    else if (type == 1 && label != "group") {

        if (selectedGroupVal != "" && selectedPartner != "" && selectedSecretary != "") {
            document.getElementById('addbuttonSecretary').classList.remove('disableBtn')
            document.getElementById('addbuttonSecretary').disabled = false


            document.getElementById('editbuttonSecretary').classList.remove('disableBtn')
            document.getElementById('editbuttonSecretary').disabled = false


        }



        if (selectedGroupVal.group == unmutableSelectedGroupVal.group && v.name == unmutableSelectedSecretary.name) {

            document.getElementById('editbuttonSecretary').disabled = true;
            document.getElementById('editbuttonSecretary').classList.add('disableBtn')
        }
        else {

            document.getElementById('editbuttonSecretary').disabled = false;
            document.getElementById('editbuttonSecretary').classList.remove('disableBtn')
        }

    }

    else if (type == 1 && label == "group") {

        if (selectedGroupVal != "" && selectedPartner != "" && selectedSecretary != "") {
            document.getElementById('addbuttonSecretary').classList.remove('disableBtn')
            document.getElementById('addbuttonSecretary').disabled = false


            document.getElementById('editbuttonSecretary').classList.remove('disableBtn')
            document.getElementById('editbuttonSecretary').disabled = false


        }



        if (selectedSecretary.name == unmutableSelectedSecretary.name && selectedGroupVal.description == unmutableSelectedGroupVal.group) {

            document.getElementById('editbuttonSecretary').disabled = true;
            document.getElementById('editbuttonSecretary').classList.add('disableBtn')
        }
        else {

            document.getElementById('editbuttonSecretary').disabled = false;
            document.getElementById('editbuttonSecretary').classList.remove('disableBtn')
        }

    }
}



const downloadLogs = () => {
    // $.getJSON('/download_admin_logs', {
    // }, (v) => {

    //     

    // })

    //     .fail((err) => {
    //         
    //     })

    if (tab == 0) {
        window.location = '/download_admin_logs'
    }
    else if (tab == 1) {
        window.location = '/download_secretary_logs'
    }


}


const loadSecretaryListTable = () => {

    $.getJSON('/load_engagement_secretary', {
    }, (v) => {

        secretaryTableList = v

        // tableList = v
        $(tblUserAccessBody).empty();
        $(tblSecretaryAccessBody).empty();


        let myrow = '';
        //
        v.forEach((d) => {

            let finalVal = JSON.stringify(d)
            myrow += `<tr>
                               
                               <td style="height: 70px">
                                     <div class="container p3">
                                        ${d.secretary}
                                     </div>
                               </td>
                               <td style="height: 70px"> 
                                    
                                     <div class="container p3">
                                      ${d.partner}
                                     </div>
                               </td>
                               
                               <td style="height: 70px">
                                     <div class="container p3">
                                     ${d.group}
                                     </div>
                               </td>
                               
                               <td style="height: 70px"> 
                                    <div class="container d-flex p-3">
                                         <span onclick='viewEngagementSecretary(${finalVal})' class="Appkit4-icon icon-pencil-fill  a-text-grey"></span>
                                        <span onclick='deleteUser(${finalVal}, "secretary")' class="Appkit4-icon icon-delete-fill ms-2 a-text-grey"></span>
                                    </div>
                               </td>
                           </tr>`
        });

        let val = escapeHtml(myrow)

        $('#tblSecretaryAccessBody').append(unescapeHtml(val)).html();


    })
}


const getGroupCodeList = () => {
    $.getJSON('/groupcodelist', {}, (data) => {


        groupCodeList = data
        groupCodeListUmuttable = data

    })
}

const selectedGroup = (val) => {

    //setting selectedgroup value
    selectedGroupVal = val;
    document.getElementById('groupInput').value = val.description
    document.getElementById('groupInputEdit').value = val.description

    countUpdate = countUpdate + 1;
    groupEdited = true;
    removeDisable(1, val, "group")
    
}



const addSecretary = () => {



    let taker = userState.employee_Name
    let action = "Added " + selectedSecretary.name + " as " + selectedPartner.name + "'s secretary for " + selectedGroupVal.description

    $.getJSON('/validate_adding_engagement_secretary', {
        //secretary_variable: selectedSecretary.id,
        //partner_variable: selectedPartner.id,
        //group_variable: selectedGroupVal.group_Code_WO_Desc,
        partner: selectedPartner.id,
        group: selectedGroupVal.group_Code_WO_Desc,


    }, (data) => {


        if (data.length != 0) {
            let getCounter = data[0].engagement_Secretary_Count

            if (getCounter >= 1) {
                //existing
                alert('Partner with the same group already exists. Please try again.')
            }
            else {
                $.getJSON('/add_engagement_secretary', {
                    secretary: selectedSecretary.id,
                    partner: selectedPartner.id,
                    group: selectedGroupVal.group_Code_WO_Desc,
                    //action_Taker: userState.username,
                    action_Taker: document.getElementById('username').innerText,
                    action_Taken: action

                }, (data) => {

                    window.scrollTo(0, 0);

                    document.getElementById('statusContainer').classList.remove('d-none')
                    document.getElementById('statusContainer').classList.add('d-block');

                    secretaryEdited = false;
                    groupEdited = false

                    //$(statusIcon).append(`<span class='a-text-white ml-3'> Secretary successfully added!</span>`)

                    // NEW
                    $('#message_notif').text('Secretary successfully added!');

                    setTimeout(() => {
                        document.getElementById('statusContainer').classList.add('d-none')
                        document.getElementById('statusContainer').classList.remove('d-flex')

                        document.getElementById('addbuttonSecretary').classList.add('disableBtn')
                        document.getElementById('addbuttonSecretary').disabled = true

                        $(statusIcon).empty();
                    }, 3000)

                    document.getElementById('myInputSecretaryUser').value = ''
                    document.getElementById('myInputPartnerUser').value = ''
                    document.getElementById('groupInput').value = ''

                    document.getElementById('myInputEngagementUser').value = ''
                    document.getElementById('addbutton').disabled = true;
                    document.getElementById("regularradio").checked = false
                    document.getElementById("adminradio").checked = false

                    loadSecretaryListTable();

                    $('#ModalRemoveUser').modal('hide');
                    $('#addSecretaryModal').modal('hide');


                }).fail((err) => {
                    document.getElementById('statusContainer').classList.remove('d-none')
                    document.getElementById('statusContainer').classList.add('d-block');
                    document.getElementById('statusContainer').style.backgroundColor = "red"
                    document.getElementById('statusIcon').classList.remove('icon-circle-checkmark-fill')
                    document.getElementById('statusIcon').classList.add('icon-circle-delete-fill')

                    document.getElementById('addbutton').disabled = false;

                    // $(statusIcon).append(`<span class='a-text-white'>Unable to add, Please try again.</span>`)

                    // NEW
                    $('#message_notif').text('Unable to add, Please try again.');


                    setTimeout(() => {
                        document.getElementById('statusContainer').classList.add('d-none')
                        document.getElementById('statusContainer').classList.remove('d-flex')
                        document.getElementById('statusContainer').style.backgroundColor = "green"
                        document.getElementById('statusIcon').classList.add('icon-circle-checkmark-fill')

                        document.getElementById('statusIcon').classList.remove('icon-circle-delete-fill')

                        $(statusIcon).empty()
                    }, 3000)


                    selectedUser = ''
                    userRole = ''
                    $('#addSecretaryModal').modal('hide');
                })
            }

        }
    })


}

const loadSecretaryLogs = () => {
    // engagement_secretary_logs

    $(tblBodyLogs).empty();
    validateAccess(0)
    $.getJSON('/engagement_secretary_logs', {
    }, (v) => {

        adminLogsList = v
        unmutableAdminLogsList = v

        let tblString = ''


        v.map((i, k) => {
            tblString = `
        <tr>
            <td>
                <div class='p-3'>${moment(i.log_Date).format('DD MMMM YYYY')}</div>
            </td>
             <td>
                <div class='p-3'>${i.log_Time_Format}</div>
            </td>
             <td>
                <div class='p-3'>${i.action_Taker}</div>
            </td>

             <td>
                <div>
                   ${i.action_Taken}
                </div>
            </td>
        </tr>
    `
            let val = escapeHtml(tblString)

            $('#tblBodyLogs').append(unescapeHtml(val)).html()
        })


    })

        .fail((err) => {

        })

}

const viewEngagementSecretary = (data) => {




    $('#editSecretaryModal').modal('show');
    editSecretaryModal = true;

    loadSecretarySearchList()
    //for search partner
    loadPartnerSearchList()

    selectedSecretary = {
        id: data.secretary_Code,
        secretary: data.secretary,
        name: data.secretary

    }

    unmutableSelectedSecretary = {
        id: data.secretary_Code,
        secretary: data.secretary,
        name: data.secretary

    }


    selectedPartner = {
        id: data.partner_Code,
        partner: data.partner,
        name: data.partner
    }


    selectedGroupVal = {
        group_Code_WO_Desc: data.group_Id,
        group: data.group
    }

    unmutableSelectedGroupVal = {
        group_Code_WO_Desc: data.group_Id,
        group: data.group
    }

    selectedItem = data.id

    document.getElementById('myInputSecretaryUserEdit').value = data.secretary
    document.getElementById('myInputPartnerUserEdit').value = data.partner
    document.getElementById('groupInputEdit').value = data.group


    $(groupListEdit).empty();


    let html = ``
    groupCodeList.map((i, k) => {

        let finalVal = JSON.stringify(i)
        let html = `    
                           <a target="_blank" class="dropdown-item" onclick='return selectedGroup(${finalVal})'>${i.description}</a>
                         `
        let val = escapeHtml(html)

        $(groupListEdit).append(unescapeHtml(val)).html();
    })



    // $.getJSON('/view_engagement_secretary', {
    //     i_variable : v.id
    // }, (data) => {
    //     

    //      selectedSecretary = data.secretary_Code;
    //      selectedPartner = data.partner_Code
    //      selectedGroupVal = data.group

    //      document.getElementById('myInputSecretaryUserEdit').value = data.name
    //      document.getElementById('myInputPartnerUserEdit').value = data.name
    //      document.getElementById('groupInputEdit').value = data.description





    // }) 
}

const deleteEngagement = (v) => {



    let taker = userState.employee_Name
    let action = "Removed " + selectedUser.secretary + " and " + selectedUser.partner + " of " + selectedUser.group + " from the list."

    console.log(userState);

    $.getJSON('/delete_engagement_secretary', {
        id: selectedUser.id,
        action_Taker: userState.username,
        action_Taken: action
    }, (data) => {

        window.scrollTo(0, 0);

        document.getElementById('statusContainer').classList.remove('d-none')
        document.getElementById('statusContainer').classList.add('d-block');

        //$(statusIcon).append(`<span class='a-text-white ml-3'> Secretary successfully deleted!</span>`)

        // NEW
        $('#message_notif').text('Secretary successfully deleted!');

        setTimeout(() => {
            document.getElementById('statusContainer').classList.add('d-none')
            document.getElementById('statusContainer').classList.remove('d-flex')

            $(statusIcon).empty();
        }, 3000)

        document.getElementById('myInputEngagementUser').value = ''
        document.getElementById('addbutton').disabled = false;
        document.getElementById("regularradio").checked = false
        document.getElementById("adminradio").checked = false

        document.getElementById('searchVal').value
        loadSecretaryListTable();

        $('#ModalRemoveUser').modal('hide');

        selectedUser = ''
    })
        .fail((err) => {
            document.getElementById('statusContainer').classList.remove('d-none')
            document.getElementById('statusContainer').classList.add('d-block');
            document.getElementById('statusContainer').style.backgroundColor = "red"
            document.getElementById('statusIcon').classList.remove('icon-circle-checkmark-fill')
            document.getElementById('statusIcon').classList.add('icon-circle-delete-fill')

            document.getElementById('addbutton').disabled = false;

            //$(statusIcon).append(`<span class='a-text-white'>Unable to delete, Please try again.</span>`)

            // NEW
            $('#message_notif').text('Unable to delete, Please try again.');


            setTimeout(() => {
                document.getElementById('statusContainer').classList.add('d-none')
                document.getElementById('statusContainer').classList.remove('d-flex')
                document.getElementById('statusContainer').style.backgroundColor = "green"
                document.getElementById('statusIcon').classList.add('icon-circle-checkmark-fill')

                document.getElementById('statusIcon').classList.remove('icon-circle-delete-fill')

                $(statusIcon).empty()
            }, 3000)


            selectedUser = ''
            userRole = ''
            $('#ModalRemoveUser').modal('hide');
        })
}