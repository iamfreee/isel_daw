import React, { Component } from 'react'
import PropTypes from 'prop-types'

import { Redirect } from 'react-router'

import { GUEST, TEACHER } from '../../models/Roles'

class Home extends Component {
  constructor(props) {
    super(props)
    this.state = {}
  }

  render() {
    const { session } = this.props
    if (session.isAuthenticated) {
      return (
        <Redirect
          to={
            session.user.hasRole(TEACHER)
              ? 'teachers/' + session.user.number
              : 'students/' + session.user.number
          }
        />
      )
    }
    
    return <Redirect to="courses" />
  }
}

Home.propTypes = {
  session: PropTypes.object.isRequired
}

export default Home
