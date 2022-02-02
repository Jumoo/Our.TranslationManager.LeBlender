(function () {

    'use strict';

    function jobsController($routeParams,
        $timeout,
        navigationService,
        localizationService,
        notificationsService,
        translateCultureService) {

        var vm = this;

        vm.page = {
            title: 'Archived Jobs : ',
            description: 'Jobs that have been completed or closed'
        };

        vm.cultureName = $routeParams.id;
        vm.statusRange = [20, 100];
        vm.loaded = true;

        getCultureInfo(vm.cultureName);

        $timeout(function () {
            navigationService.syncTree({ tree: "archive", path: ['-1', vm.cultureName] });
        });


        function getCultureInfo(cultureName) {
            translateCultureService.getCultureInfo(cultureName)
                .then(function (result) {
                    vm.page.title += result.data.DisplayName;
                });

        }
    }

    angular.module('umbraco')
        .controller('translateJobsArchiveController', jobsController);

})();